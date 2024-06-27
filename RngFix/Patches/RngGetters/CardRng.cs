using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits.Common;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches.RngGetters
{
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetRewardCards))]
    class CardReward_Patch
    {

        static RandomGen SwitchCardRng(GameRunController gr)
        {
            var sf = new StackFrame(2);
            var mName = sf.GetMethod().Name;
            var grngs = GrRngs.GetOrCreate(gr);
            switch (mName)
            {
                case nameof(Stage.GetEnemyCardReward):
                    return gr.CardRng;
                case nameof(Stage.GetEliteEnemyCardReward):
                    // 2do experimental
                    return gr.CardRng;
                    return GrRngs.GetEliteCardRng(gr);
                case nameof(Stage.GetBossCardReward):
                    return GrRngs.GetBossCardRng(gr);
                case nameof(Stage.GetDrinkTeaCardReward):
                    return grngs.ExhibitSelfRngs.GetRng(nameof(HuangyouJiqiren));
                default:
                    break;
            }

            return gr.CardRng;
        }

        static RandomGen ExtraCardRewardRng(GameRunController gr)
        {
            return GrRngs.GetOrCreate(gr).persRngs.extraCardRewardRng;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);
            int i = 0;
            while (i < 3)
            {
                matcher = matcher.ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.SwitchCardRng)));
                i++;
            }

            matcher = matcher.ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.ExtraCardRewardRng)));

            matcher = matcher.ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetCardUpgradeQueueRng)));


            return matcher.InstructionEnumeration();
        }

    }




}
