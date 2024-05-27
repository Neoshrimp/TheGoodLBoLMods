using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Adventures;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.RngGetters
{

    //2do make shop less predictable? 

    [HarmonyPatch(typeof(RinnosukeTrade), nameof(RinnosukeTrade.GetExhibitPrice))]
    class RinnosukeTrade_GetExhibitPrice_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.ShopRng), AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.AdventureRng)))
                 .InstructionEnumeration();
        }
    }


}
