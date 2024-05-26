using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static RngFix.BepinexPlugin;
using LBoL.Base;
using System.Reflection.Emit;
using LBoL.Core.Stations;
using LBoL.EntityLib.Stages.NormalStages;
using System.Reflection;

namespace RngFix.Patches
{



    [HarmonyPatch(typeof(Stage), nameof(Stage.GetEnemies))]
    class GetEnemies_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions)
                 .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.StationRng))))
                 .Set(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEnemyStationRng)))
                 //
                 .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.StationRng))))
                 .Set(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEnemyStationRng)))
                 //
                 .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.StationRng))))
                 .Set(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEnemyStationRng)))
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

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions)
                 .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.StationRng))))
                 .Set(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEliteStationRng)))
                 .InstructionEnumeration();
        }
    }

    [HarmonyPatch(typeof(Stage), nameof(Stage.GetAdventure))]
    class GetAdventure_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions)
                 .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.StationRng))))
                 .Set(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetEventStationRng)))
                 .InstructionEnumeration();
        }
    }


    // 2do can still be manipulated via kill order?
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.GenerateEnemyPoints))]
    class GenerateEnemyPoints_Patch
    {
        static RandomGen GetLootGen(GameRunController gr) => GrRngs.GetOrCreate(gr).battleLootRng;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions)
               .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.GameRunEventRng))))
               .Set(OpCodes.Call, AccessTools.Method(typeof(GenerateEnemyPoints_Patch), nameof(GenerateEnemyPoints_Patch.GetLootGen)))
               .InstructionEnumeration();
        }

    }
}
