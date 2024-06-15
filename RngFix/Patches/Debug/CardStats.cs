using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using Logging;
using System;
using System.Collections.Generic;
using System.Text;
using LBoL.Presentation;
using LBoLEntitySideloader.CustomHandlers;
using System.Linq;

namespace RngFix.Patches.Debug
{


    //[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetRewardCards))]
    class CardRewardsLog_Patch
    {
        static void Prefix(GameRunController __instance, ref float __state)
        {
            __state = __instance._cardRareWeightFactor;
        }

        static void Postfix(GameRunController __instance, Card[] __result, ref float __state)
        {
            var log = StatsLogger.GetCardLog(__instance);
            for (int i = 0; i < 4; i++)
            {
                if (i < __result.Length)
                    log.AddVal(__result[i].Name);
                else
                    log.AddVal("N/A");
            }
            log.AddVal(__state);
            log.AddVal(__instance._cardRareWeightFactor);
            log.AddVal(__instance.CardRng.State);
            log.AddVal(__instance.CurrentMap.VisitingNode.StationType);
        }
    }


    class PickCardLog
    {
        public static void RegisterOnCardsAdded()
        {
            return;
            CHandlerManager.RegisterGameEventHandler((gr) => gr.DeckCardsAdded, args =>
            {
                var log = StatsLogger.GetCardLog(StatsLogger.Gr());
                if (log.HasVals())
                {
                    log.AddVal(string.Join(" ", args.Cards.Select(c => c.Name)));
                }
            });
        }
    }

    //[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterStation))]
    class Flush_Patch
    {
        static void Prefix(GameRunController __instance)
        {
            var log = StatsLogger.GetCardLog(__instance);
            if (!log.HasVals())
                return;
            if (log.Values.Count < StatsLogger.cardsHeader.Length)
                log.AddVal("Skip");
            log.FlushVals();
        }
    }


}
