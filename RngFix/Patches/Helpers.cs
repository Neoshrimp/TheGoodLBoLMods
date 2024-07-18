using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoLEntitySideloader.Utils;
using RngFix.CustomRngs;
using RngFix.Patches.Battle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        private static int RngGuardSingle(IEnumerable<EnemyUnit> alives)
        {
            return alives.Count() <= 1 ? 1 : 0;
        }

        private static int RngGuardMany(IEnumerable<EnemyUnit> alives, int amount)
        {
            var count = alives.Count();
            return amount >= count || amount <= 0 ? 1 : 0;
        }

        private static RandomGen FakeRng()
        {
            return new RandomGen();
        }

        public static CodeMatcher RngAdvancementGuard(this CodeMatcher matcher, ILGenerator generator, CodeMatch searchBackMatch = null, bool many = false, CodeMatch[] amountMatch = null)
        {
            var currentOperand = matcher.Instruction.operand?.ToString() ?? "";
            if (!( currentOperand.Contains(nameof(CollectionsExtensions.SampleOrDefault))
                || currentOperand.Contains(nameof(CollectionsExtensions.SampleManyOrAll))
                || currentOperand.Contains(nameof(CollectionsExtensions.Sample))
                || currentOperand.Contains(nameof(CollectionsExtensions.SampleMany))
                ))
            {
                throw new ArgumentException("Matcher's position is not on sampling method");
            }
            if (searchBackMatch == null)
            {
                searchBackMatch = new CodeMatch (ci => ci.opcode == OpCodes.Callvirt && ci.operand as MethodInfo == AccessTools.PropertyGetter(typeof(EnemyGroup), nameof(EnemyGroup.Alives)));
            }
            var skipRng = generator.DefineLabel();
            var doSampling = generator.DefineLabel();
            matcher.AddLabels(new Label[] { doSampling })
                .InsertAndAdvance(new CodeInstruction(OpCodes.Br, doSampling));

            if (many)
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, "deezdeeznuts"));
            else
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.FakeRng))).WithLabels(new Label[] { skipRng }));

            matcher
                .Advance(-1) // avoids matching sampling method
                .ThrowIfNotMatchBack($"{searchBackMatch} didn't match anything", searchBackMatch)
                .MatchEndBackwards(searchBackMatch)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup));



            var amountIns = new List<CodeInstruction>();
            if (many)
            {
                if (amountMatch == null)
                { 
                    amountIns.Add(matcher.Instruction);
                }
                else
                {
                    matcher.ThrowIfNotMatchForward($"AmountMatch@{matcher.Pos}: {amountMatch} didn't match anything", amountMatch)
                        .MatchEndForward(amountMatch);
                    amountIns.AddRange(matcher.InstructionsWithOffsets(-amountMatch.Length+1, 0));
                }

                matcher.Advance(1);
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.RngGuardMany))));
            }
            else
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.RngGuardSingle))));

            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, skipRng));
            foreach ((var ci, var i) in amountIns.Select((ci, i) => (ci, i)))
            {
                matcher.InsertAndAdvance(ci.Clone());
            }



            if (many)
            {
                matcher.ThrowIfNotMatchForward("SATORRI",new CodeMatch(ci => (ci.operand?.ToString() ?? "") == "deezdeeznuts"));

                matcher.MatchEndForward(new CodeMatch(ci => (ci.operand?.ToString() ?? "") == "deezdeeznuts"));
                
                foreach ((var ci, var i) in amountIns.Select((ci, i) => (ci, i)))
                {
                    if (i == 0)
                    {
                        matcher.SetInstruction(ci.Clone())
                            .AddLabels(new Label[] { skipRng })
                            .Advance(1);
                    }
                    else
                        matcher.InsertAndAdvance(ci.Clone());
                }
                matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Helpers), nameof(Helpers.FakeRng))));
            }

            return matcher;
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



    }
}
