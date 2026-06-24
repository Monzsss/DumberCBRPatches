//EssenceConfigValues.cs

using System.Reflection;
using System.Linq.Expressions;
using MBMScripts;

namespace DumberCBRPatches.Configuration
{
    public struct LocalUpgradeAttributeInfo
    {
        public int Attribute;
        public float UpgradeValue;
        public float UpgradePointsPerValue;
    }

    public class EssenceConfigValues
    {
        public Dictionary<ETrait, List<LocalUpgradeAttributeInfo>> EssenceDataAttributes { get; internal set; } = [];
        private readonly List<List<LocalUpgradeAttributeInfo>> _listPool = [];
        private int _poolIndex;

        private List<LocalUpgradeAttributeInfo> GetPooledList()
        {
            if (_poolIndex < _listPool.Count)
            {
                var list = _listPool[_poolIndex++];
                list.Clear();
                return list;
            }
            var newList = new List<LocalUpgradeAttributeInfo>();
            _listPool.Add(newList);
            _poolIndex++;
            return newList;
        }

        private static class ReduxReflectionCache
        {
            public static readonly Func<System.Collections.IEnumerable>? GetRegistryAll;
            public static readonly Func<object, object>? GetTrait;
            public static readonly Func<object, System.Collections.IEnumerable>? GetUpgrades;
            public static readonly Func<object, object>? GetUpgradeAttribute;
            public static readonly Func<object, float>? GetUpgradeValue;
            public static readonly Func<object, float>? GetUpgradePoints;
            public static readonly Action<object, float>? SetUpgradeValue;

            static ReduxReflectionCache()
            {
                try
                {
                    Type? registryType = Type.GetType("ComplexBreedingRedux.Data.Essences.EssenceRegistry, ComplexBreedingRedux");
                    Type? essenceType = Type.GetType("ComplexBreedingRedux.Data.Essences.EssenceData, ComplexBreedingRedux");
                    Type? upgradeType = Type.GetType("ComplexBreedingRedux.Data.Essences.UpgradeAttributeInfo, ComplexBreedingRedux");

                    if (registryType == null || essenceType == null || upgradeType == null) return;

                    PropertyInfo? allProp = registryType.GetProperty("All", BindingFlags.Public | BindingFlags.Static);
                    if (allProp != null)
                    {
                        var body = Expression.Convert(Expression.Property(null, allProp), typeof(System.Collections.IEnumerable));
                        GetRegistryAll = Expression.Lambda<Func<System.Collections.IEnumerable>>(body).Compile();
                    }

                    PropertyInfo? traitProp = essenceType.GetProperty("ReplacedTrait") ?? essenceType.GetProperty("Trait");
                    PropertyInfo? upgradesProp = essenceType.GetProperty("AttributeUpgrades");

                    if (traitProp != null) GetTrait = CompileGetter<object>(essenceType, traitProp);
                    if (upgradesProp != null) GetUpgrades = CompileGetter<System.Collections.IEnumerable>(essenceType, upgradesProp);

                    PropertyInfo? attrProp = upgradeType.GetProperty("Attribute");
                    PropertyInfo? valProp = upgradeType.GetProperty("UpgradeValue");
                    PropertyInfo? ptsProp = upgradeType.GetProperty("UpgradePointsPerValue");

                    if (attrProp != null) GetUpgradeAttribute = CompileGetter<object>(upgradeType, attrProp);
                    if (valProp != null) GetUpgradeValue = CompileGetter<float>(upgradeType, valProp);
                    if (ptsProp != null) GetUpgradePoints = CompileGetter<float>(upgradeType, ptsProp);

                    if (valProp != null && valProp.CanWrite)
                    {
                        var instanceParam = Expression.Parameter(typeof(object));
                        var valueParam = Expression.Parameter(typeof(float));
                        var castInstance = Expression.Convert(instanceParam, upgradeType);
                        var assign = Expression.Assign(Expression.Property(castInstance, valProp), valueParam);
                        SetUpgradeValue = Expression.Lambda<Action<object, float>>(assign, instanceParam, valueParam).Compile();
                    }
                }
                catch (Exception ex)
                {
                    ModEntry.LogError($"Failed to build highly-optimized delegate cache expressions: {ex}");
                }
            }

            private static Func<object, T> CompileGetter<T>(Type targetType, PropertyInfo prop)
            {
                var instanceParam = Expression.Parameter(typeof(object));
                var castInstance = Expression.Convert(instanceParam, targetType);
                var propAccess = Expression.Property(castInstance, prop);
                var castResult = Expression.Convert(propAccess, typeof(T));
                return Expression.Lambda<Func<object, T>>(castResult, instanceParam).Compile();
            }
        }

        public static bool TryGetTraitFromInstance(object essenceInstance, out ETrait trait)
        {
            trait = default;
            if (ReduxReflectionCache.GetTrait == null) return false;

            var traitObj = ReduxReflectionCache.GetTrait(essenceInstance);
            if (traitObj is ETrait resolvedTrait)
            {
                trait = resolvedTrait;
                return true;
            }
            return false;
        }

        public void PopulationRegistryCacheFromRedux()
        {
            EssenceDataAttributes.Clear();
            DumberCBRPatches.Patches.AttributeUpgradesPatch.ClearCache();
            _poolIndex = 0;

            if (ReduxReflectionCache.GetRegistryAll == null || ReduxReflectionCache.GetTrait == null ||
                ReduxReflectionCache.GetUpgrades == null || ReduxReflectionCache.GetUpgradeAttribute == null ||
                ReduxReflectionCache.GetUpgradeValue == null || ReduxReflectionCache.GetUpgradePoints == null)
            {
                return;
            }

            try
            {
                var allCollection = ReduxReflectionCache.GetRegistryAll();
                if (allCollection == null) return;

                int filterMode = ModSettingsDataRegister.EssenceFilterModeData.Value;

                Type? itemType = null;

                foreach (object essence in allCollection)
                {
                    if (essence == null) continue;

                    if (TryGetTraitFromInstance(essence, out ETrait eTrait))
                    {
                        var sourceUpgrades = ReduxReflectionCache.GetUpgrades(essence);
                        if (sourceUpgrades == null) continue;

                        var processedList = GetPooledList();

                        if (itemType == null)
                        {
                            Type listType = sourceUpgrades.GetType();
                            if (listType.IsGenericType) itemType = listType.GetGenericArguments()[0];
                        }

                        System.Collections.IList? harmonyResultList = null;
                        if (itemType != null && filterMode != 0)
                        {
                            Type genericListType = typeof(List<>).MakeGenericType(itemType);
                            harmonyResultList = Activator.CreateInstance(genericListType) as System.Collections.IList;
                        }

                        PropertyInfo? attrProp = itemType?.GetProperty("Attribute");
                        PropertyInfo? valueProp = itemType?.GetProperty("UpgradeValue");
                        PropertyInfo? pointsProp = itemType?.GetProperty("UpgradePointsPerValue");

                        foreach (object upgrade in sourceUpgrades)
                        {
                            if (upgrade == null) continue;

                            var attrObj = ReduxReflectionCache.GetUpgradeAttribute(upgrade);
                            if (attrObj == null) continue;

                            string attrName = attrObj.ToString() ?? string.Empty;
                            int attributeEnumInt = (int)attrObj;

                            float upValue = ReduxReflectionCache.GetUpgradeValue(upgrade);
                            float pointsPerVal = ReduxReflectionCache.GetUpgradePoints(upgrade);

                            bool isNegativeEffect = attrName switch
                            {
                                "MaintenanceCost" => upValue > 0f,
                                "GrowthTime" or "DamageDuringSex" or "SexTime" => upValue > 0f,
                                _ => upValue < 0f
                            };

                            if (isNegativeEffect && filterMode == 1)
                            {
                                // Invert the value mathematically to make it a buff
                                upValue = -upValue;
                            }

                            LocalUpgradeAttributeInfo info;
                            info.Attribute = attributeEnumInt;
                            info.UpgradeValue = upValue;
                            info.UpgradePointsPerValue = pointsPerVal;
                            processedList.Add(info);

                            if (harmonyResultList != null && itemType != null)
                            {
                                object clonedUpgradeItem = Activator.CreateInstance(itemType);
                                attrProp?.SetValue(clonedUpgradeItem, attrObj);
                                pointsProp?.SetValue(clonedUpgradeItem, pointsPerVal);
                                valueProp?.SetValue(clonedUpgradeItem, upValue);
                                harmonyResultList.Add(clonedUpgradeItem);
                            }
                        }

                        EssenceDataAttributes[eTrait] = processedList;

                        if (harmonyResultList != null)
                        {
                            DumberCBRPatches.Patches.AttributeUpgradesPatch.UpdateCachedPayload(eTrait, harmonyResultList);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModEntry.LogError($"Error processing Redux database updates: {ex}");
            }
        }

    }
}
