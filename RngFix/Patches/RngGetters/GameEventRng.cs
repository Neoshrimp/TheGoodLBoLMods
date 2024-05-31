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

namespace RngFix.Patches.RngGetters
{

    // 2do can still be manipulated via kill order?
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.GenerateEnemyPoints))]
    class GenerateEnemyPoints_Patch
    {
        static RandomGen GetLootGen(GameRunController gr) => GrRngs.GetOrCreate(gr).battleLootRng;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
               .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.GameRunEventRng))))
               .Set(OpCodes.Call, AccessTools.Method(typeof(GenerateEnemyPoints_Patch), nameof(GenerateEnemyPoints_Patch.GetLootGen)))
               .InstructionEnumeration();
        }
    }



    [HarmonyPatch]
    class MoneyReward_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BossStation), nameof(BossStation.GenerateRewards));
            yield return AccessTools.Method(typeof(Station), nameof(Station.GenerateEliteEnemyRewards));
            yield return AccessTools.Method(typeof(Station), nameof(BossStation.GenerateEnemyRewards));
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher.ReplaceRngGetter(nameof(GameRunController.GameRunEventRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetMoneyRewardRng)));
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            return matcher.InstructionEnumeration();
        }

    }



}
