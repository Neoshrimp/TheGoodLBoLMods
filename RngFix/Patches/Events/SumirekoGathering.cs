using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Adventures;
using LBoL.EntityLib.Adventures.FirstPlace;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.Events
{
    // first roll doesn't happen if there are no rare cards in the deck
    [HarmonyPatch(typeof(SumirekoGathering), nameof(SumirekoGathering.InitVariables))]
    class SumirekoGathering_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.AdventureRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetTransitionRng)))
                 .LeaveJumpFix().InstructionEnumeration();
        }
    }


}
