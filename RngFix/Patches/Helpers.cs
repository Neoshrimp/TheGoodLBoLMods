using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches
{
    public static class Helpers
    {
        public static CodeMatcher ReplaceRngGetter(this CodeMatcher matcher, string targetName, MethodInfo newCall)
        {
            var targetGetter = AccessTools.PropertyGetter(typeof(GameRunController), targetName);
            if (targetGetter == null)
                throw new ArgumentException($"Could not find {nameof(GameRunController)}.{targetGetter}");
            var matchFor = new CodeMatch ((ci) => ci.operand as MethodInfo == targetGetter);

            return matcher.MatchForward(false, matchFor)
                 .ThrowIfNotMatch($"{targetName} getter not matched.", matchFor)
                 .Set(OpCodes.Call, newCall);
        }

        public static bool IsActTransition(MapNode node) => 
                   node.StationType is StationType.Entry 
                || node.StationType is StationType.Select
                || node.StationType is StationType.Supply
                || node.StationType is StationType.Trade;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <param name="valInit"></param>
        /// <param name="value"></param>
        /// <returns>True if value was present</returns>
        public static bool GetOrCreateVal<K, V>(this Dictionary<K, V> dic, K key, Func<V> valInit, out V value) 
        {
            if (dic.TryGetValue(key, out value))
                return true;
            value = valInit();
            dic[key] = value;
            return false;
        }
    }
}
