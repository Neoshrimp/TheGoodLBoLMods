using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Stages.NormalStages;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using LBoL.EntityLib.EnemyUnits.Lore;
using LBoL.EntityLib.Adventures.FirstPlace;

namespace RngFix.Patches.RngGetters
{
    [HarmonyPatch(typeof(Stage), nameof(Stage.GetEnemies))]
    class GetEnemies_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var newCall = AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEnemyActQueueRng));
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.StationRng), newCall)
                .ReplaceRngGetter(nameof(GameRunController.StationRng), newCall)
                .ReplaceRngGetter(nameof(GameRunController.StationRng), newCall)
                .InstructionEnumeration();
        }
    }

    [HarmonyPatch]
    class GetEliteEnemies_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Stage), nameof(Stage.GetEliteEnemies));
            yield return AccessTools.Method(typeof(BambooForest), nameof(BambooForest.GetEliteEnemies));

        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.StationRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEliteQueueRng)))
                .InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(Stage), nameof(Stage.GetAdventure))]
    class GetAdventure_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.StationRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetAdventureQueueRng)))
                 .InstructionEnumeration();
        }
    }

}
