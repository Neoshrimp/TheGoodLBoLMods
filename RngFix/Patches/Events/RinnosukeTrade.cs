using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Adventures;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.Events
{

    // exhibits to sell are rolled using AdventureRng which's state afterwards will depend on number of losable exhibits player has. Price modifiers are rolled using different rng ensure they are they consistent for the same seed.
    [HarmonyPatch(typeof(RinnosukeTrade), nameof(RinnosukeTrade.GetExhibitPrice))]
    class RinnosukeTrade_GetExhibitPrice_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.ShopRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetTransitionRng)))
                 .LeaveJumpFix().InstructionEnumeration();
        }
    }
}
