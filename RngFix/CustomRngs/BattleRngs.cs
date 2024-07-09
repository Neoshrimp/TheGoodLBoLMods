using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.Cards.Neutral.White;
using LBoL.Presentation;
using RngFix.CustomRngs.Sampling;
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.CustomRngs.Sampling.UniformPools;
using RngFix.Patches;
using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static RngFix.BepinexPlugin;

namespace RngFix.CustomRngs
{
    // shuffling
    // card discovery
    // enemy move rng (killing an enemy vs not killing an enemy might result in different amount of rng advances)
    // random enemy targeting
    // random card targeting
    // conditional advancement of rng (old kokoro effect)
    public class BattleRngs
    {
        public static BattleController Battle() => GameMaster.Instance?.CurrentGameRun?.Battle;

        private static IEnumerable<Type> paddedCards = null;
        private static RepeatableUniformRandomPool<Type> infiteDeck = null;



        public static IEnumerable<Type> PaddedCards
        {
            get
            {
                if (paddedCards == null)
                    paddedCards = Padding.CardPadding(CardConfig.AllConfig());
                return paddedCards;
            }
        }

        public static RepeatableUniformRandomPool<Type> InfiteDeck 
        {
            get
            {
                if (infiteDeck == null)
                { 
                    infiteDeck = new RepeatableUniformRandomPool<Type>();
                    infiteDeck.AddRange(PaddedCards);
                }
                return infiteDeck;
            }
        }

        public static List<Card> Shuffle(RandomGen rng, IList<Card> toShuffle)
        {
            var slotRng = new RandomGen(rng.NextULong());
            var indexRootRng = new RandomGen(slotRng.NextULong());

            if (toShuffle.Count <= 1)
                return toShuffle.ToList();


            var typeSlots = new Dictionary<Type, List<int>>();
            var cardBins = new Dictionary<Type, List<Card>>();

            foreach (var card in toShuffle)
            {
                var cType = card.GetType();
                cardBins.GetOrCreateVal(cType, () => new List<Card>(), out var cards);
                cards.Add(card);
            }




            var potentialCards = new InCountedPool<Type>(toShuffle.Select(c => c.GetType()));





            // 1st pass: shuffle by card type preserving card relative order
            int rolls = 0;
            int index = 0;
            while (potentialCards.Count > 0)
            {
                var cType = InfiteDeck.Sample(slotRng);
                if (cType != null && potentialCards.IsSatisfied(cType))
                {
                    typeSlots.GetOrCreateVal(cType, () => new List<int>(), out var ctIndexes);
                    ctIndexes.Add(index);

                    potentialCards.ReduceCount(cType);
                    index++;
                }

                rolls++;
            }
            log.LogDebug($"infinite rolls: {rolls}");


            var rez = Enumerable.Repeat<Card>(null, toShuffle.Count).ToList();

            int totalGRolls = 0;
            // 2nd pass assign concrete cards to type slots
            foreach (var type in typeSlots.Keys)
            {
                var indexRng = new RandomGen(indexRootRng.NextULong());
                var groupRng = new RandomGen(indexRng.NextULong());

                var indexes = typeSlots[type];

                if (indexes.Count == 1)
                {
                    rez[indexes[0]] = cardBins[type].First();
                    continue;
                }


                //var encodedGroups = new List<IEnumerable<Card>>(Enumerable.Repeat<IEnumerable<Card>>(null, totalGroups));
                //var groupCount = new List<int>();
                var groupsSample = new RepeatableUniformRandomPool<UniformUniqueRandomPool<Card>>();
                groupsSample.AddRange(Enumerable.Repeat<UniformUniqueRandomPool<Card>>(null, totalGroups));
                int singleGroupIndex = -1;

                foreach (var g in EncodeCardsByProperties(cardBins[type]))
                {
                    var count = g.Count();
                    if (count > 0)
                    {
                        var cards = g.AsEnumerable();
                        if (count > 1)
                            cards = cards.OrderBy(c => c.InstanceId).PadEnd(20);
                        groupsSample[g.Key] = new UniformUniqueRandomPool<Card>(cards);
                    }
                    if (singleGroupIndex == -1)
                        singleGroupIndex = g.Key;
                    else
                        singleGroupIndex = -2;
                }


                
                int ii = 0;
                int gRolls = 0;
                while (ii < indexes.Count)
                {
                    UniformUniqueRandomPool<Card> groupPool;
                    if (singleGroupIndex >= 0)
                        groupPool = groupsSample[singleGroupIndex];
                    else
                    { 
                        groupPool = groupsSample.Sample(indexRng);
                        gRolls++;
                    }
                    if (groupPool != null && groupPool.NonNullCount > 0)
                    {
                        Card card = null;
                        while (card == null)
                            card = groupPool.Sample(groupRng);

                        rez[indexes[ii]] = card;
                        ii++;
                    }
                }

                totalGRolls += gRolls;
            }

            log.LogDebug("grolls: " + totalGRolls);


            return rez;

        }

        public const int upgradeGroups = 2;
        public static readonly int costGroups = 10;
        public static readonly int keywordGroups = 8 + 1;
        public static readonly int totalGroups = upgradeGroups * costGroups * keywordGroups;
        public static readonly int totalTrackedKeywords = (int)Math.Log(keywordGroups - 1, 2);
        //public static readonly int

        public static IEnumerable<IGrouping<int, Card>> EncodeCardsByProperties(IEnumerable<Card> collection)
        {

            var representedKeywords =  Enumerable.Repeat(Keyword.None, totalTrackedKeywords).ToArray();

            var groupsByProperties = collection
                .GroupBy(card =>
                {
                    int add = (card.IsUpgraded ? 1 : 0) * ((totalGroups / upgradeGroups) - 1);

                    add = add + (card.Cost.Amount < costGroups ? card.Cost.Amount : costGroups) * ((totalGroups / upgradeGroups / costGroups) - 1);


                    var maskedKeywords = (card.Keywords ^ (card.IsUpgraded ? card.Config.UpgradedKeywords : card.Config.Keywords));

                    var kGroupCode = 0;
                    var kBits = Enumerable.Repeat(0, totalTrackedKeywords).ToArray();
                    if (maskedKeywords > 0)
                    {
                        foreach (var o in Enum.GetValues(typeof(Keyword)))
                        {
                            var k = (Keyword)o;
                            if (maskedKeywords.HasFlag(k))
                            {
                                for (int i = representedKeywords.Count() - 1; i >= 0; i--)
                                {
                                    if (representedKeywords[i] == k || representedKeywords[i] == Keyword.None)
                                    {
                                        maskedKeywords ^= k;
                                        representedKeywords[i] = k;
                                        kBits[i] = 1;
                                        break;
                                    }
                                }
                            }

                        }
                        if (maskedKeywords == 0)
                            kGroupCode = Helpers.BitArrayToInt(kBits);
                        else
                            kGroupCode = keywordGroups;
                    }


                    add = add + (kGroupCode);// * ((totalG / upgradeGn / costGn / keywordGn));

                    return add;
                });

            return groupsByProperties;
        }


    }
}
