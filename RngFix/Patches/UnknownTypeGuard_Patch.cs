using HarmonyLib;
using LBoL.Core.Randoms;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;


namespace RngFix.Patches
{
    [HarmonyPatch(typeof(CardTypeWeightTable), nameof(CardTypeWeightTable.WeightFor))]
    class UnknownTypeGuard_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(OpCodes.Throw)
                .SetInstruction(new CodeInstruction(OpCodes.Pop))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_R4, 0f))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ret))

                .InstructionEnumeration();
        }

    }
}
