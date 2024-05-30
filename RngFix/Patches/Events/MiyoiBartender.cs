using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Adventures.FirstPlace;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.Events
{
    [HarmonyPatch(typeof(MiyoiBartender), nameof(MiyoiBartender.InitVariables))]
    class MiyoiBartender_InitVariables_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.StationRng), AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.AdventureRng)))
                 .InstructionEnumeration();
        }
    }
}
