using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.RngGetters
{

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetRewardCards))]
    class BossCardReward_Patch
    {

        static RandomGen GetBossCardRng(GameRunController gr)
        {
            if(gr.CurrentMap.VisitingNode.StationType == StationType.Boss)
                return GrRngs.GetBoobossCardRewardRng(gr);
            return gr.CardRng;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher.ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(BossCardReward_Patch), nameof(BossCardReward_Patch.GetBossCardRng)));
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
