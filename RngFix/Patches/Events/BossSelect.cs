using HarmonyLib;
using LBoL.Core;
using LBoL.Presentation;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RngFix.Patches.Events
{
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetOpponentCandidates))]
    class GameRunController_GetOpponentCandidates_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.StationRng), AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.AdventureRng)))
                 .LeaveJumpFix().InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    class GameMaster_SelectStationFlow_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(GameMaster), nameof(GameMaster.SelectStationFlow));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.RootRng), AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.AdventureRng)))
                .LeaveJumpFix().InstructionEnumeration();
        }


    }
}
