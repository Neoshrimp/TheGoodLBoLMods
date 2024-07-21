using HarmonyLib;
using LBoL.Base;
using LBoL.Core.Randoms;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.EntityLib.Stages.NormalStages;
using RngFix.Patches.Battle;
using RngFix.Patches.RngGetters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches.Stages
{
    [HarmonyPatch(typeof(BambooForest), nameof(BambooForest.GetEliteEnemies))]
    class BambooForest_EliteSample_Patch
    {


        private static string SampleFirstElite(UniqueRandomPool<string> _, RandomGen rng, BambooForest stage)
        {
            var pool = new UniqueRandomPool<string>(stage.EliteEnemyPool);
            var subRng = new RandomGen((ulong)rng.NextInt(0, int.MaxValue)); // advance rng only once


            var rez = "Aya";

            if (pool._entries.All(e => e.Elem == "Aya") && pool._fallbackPool.All(e => e.Elem == "Aya"))
                return rez;

            while (rez == "Aya")
                rez = pool.Sample(subRng);

            return rez;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)

                .SearchForward(ci => ci.opcode == OpCodes.Callvirt && ci.operand.ToString().Contains("Sample"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BambooForest_EliteSample_Patch), nameof(BambooForest_EliteSample_Patch.SampleFirstElite))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))


                .InstructionEnumeration();
        }
    }
}
