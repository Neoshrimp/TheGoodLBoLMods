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
using static RngFix.BepinexPlugin;

namespace RngFix.Patches.RngGetters
{
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetRewardCards))]
    //[HarmonyDebug]
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

        static RandomGen UpgradeRng(GameRunController gr)
        {
            var rootUrng = GrRngs.GetOrCreate(gr).persRngs.cardUpgradeQueueRng;
            return new RandomGen(rootUrng.NextULong());

        }

        static float UpgradeFloat(RandomGen rng)
        {
            var rez = rng.NextFloat(0f, 1f);
            return rez;
        }

        static bool CheckRepeatRareBoss(bool repeat, bool boss) => repeat && !boss;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions);


            matcher = matcher.MatchEndForward(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameRunController), nameof(GameRunController._cardRewardDecreaseRepeatRare))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_S, 6))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.CheckRepeatRareBoss))))
                ;



            int i = 0;
            while (i < 3)
            {
                matcher = matcher.ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.SwitchCardRng)));
                i++;
            }



            matcher = matcher.ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.ExtraCardRewardRng)));

            var upgradeMiniRootRng_local = generator.DeclareLocal(typeof(RandomGen));
            var randomFloat_local = generator.DeclareLocal(typeof(float));

            matcher.MatchEndForward(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.CardRng))))
                .MatchEndBackwards(new CodeInstruction(OpCodes.Ldarg_0))
                .RemoveInstructions(5)
                .Insert(new CodeInstruction(OpCodes.Ldloc, randomFloat_local.LocalIndex))

                .MatchEndBackwards(new CodeMatch(OpCodes.Br))
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.UpgradeRng))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc, upgradeMiniRootRng_local.LocalIndex))

                .MatchEndForward(new CodeInstruction(OpCodes.Ldloca_S))
                .Advance(1)

                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc, upgradeMiniRootRng_local.LocalIndex))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CardReward_Patch), nameof(CardReward_Patch.UpgradeFloat))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc, randomFloat_local.LocalIndex))

                ;



            return matcher.InstructionEnumeration();
        }

    }




}
