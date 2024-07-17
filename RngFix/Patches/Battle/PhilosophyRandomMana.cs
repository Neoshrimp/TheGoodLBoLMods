using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using RngFix.CustomRngs.Sampling.Pads;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches.Battle
{
    [HarmonyPatch(typeof(ConvertManaAction), nameof(ConvertManaAction.PhilosophyRandomMana))]
    class PhilosophyRandomMana_Patch
    {
        private static ManaColor[] SampleMana(IEnumerable<ManaColor> pool, int amount, RandomGen rng)
        {
            var samplingPool = Padding.PadManaColours(pool);
            samplingPool.Shuffle(rng);

            var rez = new List<ManaColor>();
            int a = 0;
            foreach (var c in samplingPool)
            {
                if (c == null)
                    continue;
                rez.Add(c.Value);
                a++;
                if (a >= amount)
                    break;
            }
            return rez.ToArray();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions).SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("SampleManyOrAll"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PhilosophyRandomMana_Patch), nameof(PhilosophyRandomMana_Patch.SampleMana))))
                .InstructionEnumeration();
        }


    }
}
