﻿using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Presentation.UI.Widgets;
using RngFix.CustomRngs;
using RngFix.CustomRngs.Sampling;
using RngFix.Patches.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using LBoL.EntityLib.Cards.Neutral.Red;
using LBoL.Core.Adventures;
using LBoL.Core.Stations;

namespace RngFix.Patches
{


    // AGGRO PREFIX FALSE
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollNormalExhibit))]
    [HarmonyPriority(Priority.Low)]
    class ExhibitRoll_Patch
    {
        static bool Prefix(GameRunController __instance, ref Exhibit __result, RandomGen rng, ExhibitWeightTable weightTable, Func<Exhibit> fallback, Predicate<ExhibitConfig> filter)
        {
            var gr = __instance;
            var grngs = GrRngs.GetOrCreate(gr);

            grngs.NormalExSampler.successAction = ex => gr.ExhibitPool.Remove(ex.GetType());

            __result = grngs.NormalExSampler.Roll(
                rng: rng,
                getW: (t) => weightTable.WeightFor(ExhibitConfig.FromId(t.Name)) * Library.WeightForExhibit(t, gr),
                logInfo: out var logInfo,
                filter: (t) => filter == null || filter(ExhibitConfig.FromId(t.Name)),
                fallback: fallback);

            grngs.NormalExSampler.successAction = null;


            StatsLogger.LogEx(__result, gr, logInfo);


            return false;

        }
    }


    // AGGRO PREFIX FALSE
    [HarmonyPatch(typeof(Stage), nameof(Stage.GetAdventure))]
    [HarmonyPriority(Priority.Low)]
    class AdventureRoll_Patch
    {
        static bool Prefix(Stage __instance, ref Type __result)
        {
            var stage = __instance;
            var gr = stage.GameRun;
            var grngs = GrRngs.GetOrCreate(gr);
            var adventurePoolReq = grngs.AdventureSampler.requirements.Find(r => r is AdventureInPool) as AdventureInPool;

            var t2W = new Dictionary<Type, float>();
            stage.AdventurePool.Do(e => t2W.TryAdd(e.Elem, e.Weight));

            adventurePoolReq.poolSet = new HashSet<Type>(t2W.Keys);

            grngs.AdventureSampler.successAction = (t) => stage.AdventurePool.Remove(t, true);

            __result = grngs.AdventureSampler.Roll(GrRngs.GetAdventureQueueRng(gr),
                (t) => {
                    float w = 0f;
                    t2W.TryGetValue(t, out w);
                    return w * Library.WeightForAdventure(t, gr);
                },
                logInfo: out var logInfo,
                fallback: () => typeof(FakeAdventure)
                );


            StatsLogger.LogEvent(__result, gr, logInfo);

            return false;
        }
    }



    // AGGRO PREFIX FALSE
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollCards), new Type[] { typeof(RandomGen ), typeof(CardWeightTable), typeof(int) , typeof(ManaGroup?), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(Predicate<CardConfig>) })]
    [HarmonyPriority(Priority.Low)]
    class CardRoll_Patch
    {
        static bool Prefix(GameRunController __instance, ref Card[] __result, RandomGen rng, CardWeightTable weightTable, int count, ManaGroup? manaLimit, bool colorLimit, bool applyFactors = false, bool battleRolling = false, bool ensureCount = false, [MaybeNull] Predicate<CardConfig> filter = null)
        {

            var gr = __instance;
            var grngs = GrRngs.GetOrCreate(gr);
            var rolledCards = new List<Card>();

            var cardPoolReq = grngs.RewardCardSampler.requirements.Find(r => r is CardInPool) as CardInPool;

            cardPoolReq.poolSet = new HashSet<Type>(gr.CreateValidCardsPool(weightTable, manaLimit, colorLimit, applyFactors, battleRolling, filter).Select(re => re.Elem));


            HashSet<string> charExSet = new HashSet<string> { gr.Player.Id };
            if (gr.AllCharacterCardsFlag > 0)
            {
                foreach (PlayerUnitConfig pc in PlayerUnitConfig.AllConfig())
                {
                    charExSet.Add(pc.Id);
                }
            }
            else
            {
                foreach (Exhibit exhibit in gr.Player.Exhibits)
                {
                    if (exhibit.OwnerId != null)
                    {
                        charExSet.Add(exhibit.OwnerId);
                    }
                }
            }

            Func<Type, float> getW = t => {
                var cc = CardConfig.FromId(t.Name);
                var wt = weightTable.WeightFor(cc, gr.Player.Id, charExSet);
                var bw = gr.BaseCardWeight(cc, false);
                return wt * bw;
            };

            var logger = gr.Battle == null ? StatsLogger.GetCardLog(gr) : StatsLogger.GetCardGenLog(gr);

            for (var i = 0; i < count; i++)
            {

                var prevState = rng.State;
                var card = grngs.RewardCardSampler.Roll(rng, getW, logInfo: out var logInfo, filter: t => rolledCards.All(c => c.GetType() != t));


                if (card == null)
                    if (ensureCount)
                    {
                        BepinexPlugin.log.LogWarning("ENSURING COUNT ENSURING COUNT ENSURING COUNT");
                        rng.State = prevState;
                        card = grngs.RewardCardSampler.Roll(rng, getW, logInfo: out logInfo);
                    }
                    else
                    {
                        StatsLogger.LogCard(logger, card, gr, logInfo);
                        break;
                    }

                if(card != null)
                    rolledCards.Add(card);

                StatsLogger.LogCard(logger, card, gr, logInfo);

            }


            __result = rolledCards.ToArray();
            return false;
        }
       
    }





}
