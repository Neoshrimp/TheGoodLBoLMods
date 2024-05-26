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

namespace RngFix.Patches
{


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




    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterStation))]
    class GameRunController_EnterStation_Patch
    {
        static void Prefix(GameRunController __instance)
        {
            log.LogDebug("entering node...");
            var battleRng = GrRngs.GetOrCreate(__instance).rootBattleRng;

            GrRngs.AssignBattleRngs(__instance, () => new RandomGen(battleRng.NextULong()));

        }
    }




}
