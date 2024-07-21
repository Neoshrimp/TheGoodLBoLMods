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
