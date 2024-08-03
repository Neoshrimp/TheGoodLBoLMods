using HarmonyLib;
using LBoL.Core;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.Events
{

    [HarmonyPatch(typeof(Stage), nameof(Stage.GetSupplyExhibit))]
    class GetSupplyExhibit_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.ExhibitRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetTransitionRng))) // could be adventure rng as well
                 .LeaveJumpFix().InstructionEnumeration();
        }
    }


}
