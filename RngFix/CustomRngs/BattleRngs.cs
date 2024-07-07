using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
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
using static RngFix.BepinexPlugin;

namespace RngFix.CustomRngs
{
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

        public static List<Card> Shuffle(RandomGen rng, List<Card> toShuffle)
        {
            var slotRng = new RandomGen(rng.NextULong());
            var indexRng = new RandomGen(slotRng.NextULong());


            // type to indexes
            var typeSlots = new Dictionary<Type, List<int>>();
            var typeBins = new Dictionary<Type, List<Card>>();

            foreach (var card in toShuffle)
            {
                var cType = card.GetType();
                typeBins.GetOrCreateVal(cType, () => new List<Card>(), out var cards);
                cards.Add(card);
            }

            //var poolReq = (InPool<int>)typeSampler.requirements.First(r => r is InPool<int>);

            var availableSlot = new HashSet<int>(Enumerable.Range(0, toShuffle.Count));
            var slotShuffler = new UniformUniqueRandomPool<int>();
            slotShuffler.AddRange(Enumerable.Range(0, Math.Max(9999, toShuffle.Count)));



            //log.LogDebug(string.Join(";", poolReq.countedPool.Select(kv => $"{kv.Key.Name}:{kv.Value}")));



            var potentialCards = new InCountedPool<Type>(toShuffle.Select(c => c.GetType()));

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


            //var orderedCards = toShuffle.OrderBy(c => c.Config.Index).Select((c, i) => new KeyValuePair<int, Card>(i, c));
            //var index2Card = new Dictionary<int, Card>(orderedCards);
            //var slots2ActualOrder = new Dictionary<int, int>();

/*            int o = 0;
            while (availableSlot.Count > 0)
            {
                var slot = slotShuffler.Sample(slotRng);
                if (availableSlot.Contains(slot))
                {
                    var card = index2Card[slot];
                    var cType = card.GetType();
                    typeBins.GetOrCreateVal(cType, () => new List<Card>(), out var cards);
                    cards.Add(card);

                    slots2ActualOrder.Add(slot, o);

                    typeSlots.GetOrCreateVal(cType, () => new List<int>(), out var ctIndexes);
                    ctIndexes.Add(slot);
                    availableSlot.Remove(slot);
                    o++;
                }
            }*/
            //log.LogDebug("index2card: " + string.Join(";", index2Card.Select(kv => $"{kv.Key}:{kv.Value.Name}")));
//            log.LogDebug("typeSlots: " + string.Join(";", typeSlots.Select(kv => $"{kv.Key.Name}:{string.Join(",", kv.Value)}")));
            //log.LogDebug("typeBins: " + string.Join(";", typeBins.Select(kv => $"{kv.Key.Name}:{string.Join(",", kv.Value.Select(c => c.Name))}")));

            //log.LogDebug("slots2ActualOrder: " +  string.Join(";", slots2ActualOrder.Keys));

/*            foreach(var card in orderedCards)
            {
                var cType = card.GetType();
                typeBins.GetOrCreateVal(cType, () => new List<Card>(), out var cards);
                cards.Add(card);

                var rolledSlot = typeSampler.Roll(slotRng, null, out var samplerLogInfo);
                log.LogDebug(rolledSlot);


                typeSlots.GetOrCreateVal(cType, () => new List<int>(), out var ctIndexes);
                ctIndexes.Add(rolledSlot);
                poolReq.poolSet.Remove(rolledSlot);
            }*/

            var rez = Enumerable.Repeat<Card>(null, toShuffle.Count).ToList();

            // 2nd pass assign concrete cards to type slots
            foreach ((var type, var indexes) in typeSlots)
            {
                var indexSampler = new UniformSlotSampler<Card, Card>(
                        requirements: new List<ISlotRequirement<Card>>() { new InCountedPool<Card>(typeBins[type]) },
                        initAction: c => c,
                        successAction: null,
                        failureAction: () => log.LogDebug("index deez"),
                        potentialPool: typeBins[type].OrderBy(c => c.InstanceId).PadEnd(1000)
                    );
                var countedPoolReq2 = (InCountedPool<Card>)indexSampler.requirements.First(r => r is InCountedPool<Card>);

                foreach (int i in indexes)
                {
                    var rolledCard = indexSampler.Roll(indexRng, null, out var samplerLogInfo);
                    rez[i] = rolledCard;
                    countedPoolReq2.ReduceCount(rolledCard);
                }
            }

            return rez;

        }

    }
}
