using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static RngFix.BepinexPlugin;

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

        public static int BitArrayToInt(int[] bitArray)
        {
            int result = 0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i] == 1)
                {
                    result |= (1 << i);
                }
            }
            return result;
        }

        // Brian Kernighan
        public static int CountSetBits(ulong n)
        {
            int count = 0;
            while (n > 0)
            {
                n &= (n - 1);
                count++;
            }
            return count;
        }


        // FNV-1a 64-bit constants
        private const ulong FNVOffsetBasis = 0xcbf29ce484222325;
        private const ulong FNVPrime = 0x100000001b3;

        public static ulong ToUniqueULong(this string str)
        {
            if (str.Length > 64)
                throw new ArgumentException("String length should not exceed 64 characters.");

            ulong hash = FNVOffsetBasis;

            foreach (char c in str)
            {
                hash ^= (byte)c;
                unchecked { hash *= FNVPrime; }
                
            }

            return hash;
        }

        public static bool IsGameEntity(Type type) => type.IsSubclassOf(typeof(GameEntity)) || type.IsSubclassOf(typeof(Adventure)) || type == typeof(GameEntity) || type == typeof(Adventure);

        /// <summary>
        /// Doesn't return if method is declared as a static in unrelated class
        /// </summary>
        /// <param name="stackDepth"></param>
        /// <returns></returns>
        public static Type FindCallingEntity(int stackDepth = 3)
        {
            var f2m = new StackFrame(stackDepth).GetMethod();
            var caller = f2m.DeclaringType;

/*            var st = new StackTrace();
            log.LogDebug($"---------------");
            int i = 0;
            foreach (var f in st.GetFrames())
            {
                log.LogDebug($"{i}:{f.GetMethod().DeclaringType.FullName}::{f.GetMethod().Name}");
                i++;
            }
            log.LogDebug($"---------------");*/


            //log.LogDebug($"before: {caller.Name}::{f2m.Name}|{caller.BaseType.Name}");
            while (caller != null
                && caller.IsNested
                && !IsGameEntity(caller)
                )
                caller = caller.DeclaringType;

/*            if (caller != null)
                log.LogDebug($"{caller.Name}|{caller.BaseType.Name}");
            else
                log.LogDebug("null");*/

            return caller;
        }
    }
}
