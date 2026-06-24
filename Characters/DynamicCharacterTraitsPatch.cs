//DynamicCharacterTraitsPatch.cs

using System.Reflection;
using DumberCBRPatches.Configuration;
using HarmonyLib;
using MBMScripts;

namespace DumberCBRPatches.Characters
{
    [HarmonyPatch]
    public static class DynamicCharacterTraitsPatch
    {
        public static CharacterConfigValues? Config { get; set; }

        private static readonly Dictionary<Type, CharacterInvokers> InvokerCache = [];

        private class CharacterInvokers
        {
            public Action<object>? ClearRaceTrait;
            public Action<object>? ClearTrait;
            public Action<object, ETrait>? AddTrait;
            public Action<object, ETrait, float>? AddTraitValueFloat;
            public Action<object, ETrait, int>? AddTraitValueInt;
            public Action<object, string>? SetDisplayName;
            public Action<object, int>? SetTitsType;
        }

        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            var targetClassNames = ModSettingsDataRegister.TargetCharacters;
            var methods = new List<MethodBase>();

            foreach (var rawName in targetClassNames)
            {
                Type? t = Type.GetType($"MBMScripts.{rawName}, MBMScripts")
                          ?? Type.GetType($"MBMScripts.{rawName}, Assembly-CSharp");

                if (t == null)
                {
                    string cleanName = ModSettingsDataRegister.ToDisplayNameHelper(rawName);
                    t = Type.GetType($"MBMScripts.{cleanName}, MBMScripts")
                        ?? Type.GetType($"MBMScripts.{cleanName}, Assembly-CSharp");
                }

                if (t == null) continue;

                var method = AccessTools.Method(t, "InitializeTrait");
                if (method != null)
                {
                    methods.Add(method);
                    PrecomputeInvokers(t);
                }
            }
            return methods;
        }

        private static CharacterInvokers PrecomputeInvokers(Type type)
        {
            if (InvokerCache.TryGetValue(type, out var cached)) return cached;

            var invokers = new CharacterInvokers();

            var mClearRace = AccessTools.Method(type, "ClearRaceTrait");
            if (mClearRace != null) invokers.ClearRaceTrait = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), null, mClearRace, false) ?? (obj => mClearRace.Invoke(obj, null));

            var mClearTrait = AccessTools.Method(type, "ClearTrait");
            if (mClearTrait != null) invokers.ClearTrait = (Action<object>)Delegate.CreateDelegate(typeof(Action<object>), null, mClearTrait, false) ?? (obj => mClearTrait.Invoke(obj, null));

            var mAddTrait = AccessTools.Method(type, "AddTrait", [typeof(ETrait)]);
            if (mAddTrait != null) invokers.AddTrait = (Action<object, ETrait>)Delegate.CreateDelegate(typeof(Action<object, ETrait>), null, mAddTrait, false) ?? ((obj, t) => mAddTrait.Invoke(obj, [t]));

            var mAddTraitVal = AccessTools.Method(type, "AddTraitValue", [typeof(ETrait), typeof(int)]);
            if (mAddTraitVal != null)
            {
                invokers.AddTraitValueInt = (obj, t, v) => mAddTraitVal.Invoke(obj, [t, v]);
            }
            else
            {
                var mAddTraitValFloat = AccessTools.Method(type, "AddTraitValue", [typeof(ETrait), typeof(float)]);
                if (mAddTraitValFloat != null) invokers.AddTraitValueFloat = (obj, t, v) => mAddTraitValFloat.Invoke(obj, [t, v]);
            }

            var pDisplay = AccessTools.Property(type, "DisplayName");
            if (pDisplay?.GetSetMethod() != null) invokers.SetDisplayName = (Action<object, string>)Delegate.CreateDelegate(typeof(Action<object, string>), null, pDisplay.GetSetMethod(), false) ?? ((obj, v) => pDisplay.SetValue(obj, v));

            var pTits = AccessTools.Property(type, "TitsType");
            if (pTits?.GetSetMethod() != null) invokers.SetTitsType = (Action<object, int>)Delegate.CreateDelegate(typeof(Action<object, int>), null, pTits.GetSetMethod(), false) ?? ((obj, v) => pTits.SetValue(obj, v));

            InvokerCache[type] = invokers;
            return invokers;
        }

        [HarmonyPrefix]
        public static bool Prefix(object __instance)
        {
            if (Config == null || __instance == null) return true;

            Type instanceType = __instance.GetType();

            if (!InvokerCache.TryGetValue(instanceType, out var invokers))
            {
                invokers = PrecomputeInvokers(instanceType);
            }

            string lookupKey = instanceType.Name;
            if (!Config.Values.TryGetValue(lookupKey, out var entry)) return true;

            try
            {
                invokers.ClearRaceTrait?.Invoke(__instance);
                invokers.ClearTrait?.Invoke(__instance);
            }
            catch
            {
                return true;
            }

            invokers.SetDisplayName?.Invoke(__instance, entry.CharacterName);
            invokers.SetTitsType?.Invoke(__instance, entry.TitsType);

            var traitsDict = entry.ParseTraitsAsDictionary();

            foreach (var kv in traitsDict)
            {
                if (kv.Value > 0f)
                {
                    invokers.AddTrait?.Invoke(__instance, kv.Key);

                    if (invokers.AddTraitValueInt != null)
                    {
                        invokers.AddTraitValueInt(__instance, kv.Key, (int)kv.Value);
                    }
                    else
                    {
                        invokers.AddTraitValueFloat?.Invoke(__instance, kv.Key, kv.Value);
                    }
                }
            }

            return false;
        }
    }
}
