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
using RngFix.Patches;
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

        public static IEnumerable<Type> PaddedCards
        {
            get
            {
                if (paddedCards == null)
                    paddedCards = Padding.CardPadding(CardConfig.AllConfig());
                return paddedCards;
            }
        }

        public static List<Card> Shuffle(RandomGen rng, List<Card> toShuffle)
        {
            var slotRng = new RandomGen(rng.NextULong());
            var indexRng = new RandomGen(slotRng.NextULong());

            // 1st pass slot types to indexes
            var typeSampler = new UniformSlotSampler<Type, Type>(
                    requirements: new List<ISlotRequirement<Type>>() { new InCountedPool<Type>(toShuffle.Select(c => c.GetType())) },
                    initAction: t => t,
                    successAction: null,
                    failureAction: () => log.LogDebug("shuffle deez"),
                    potentialPool: PaddedCards
                );

            // type to indexes
            var typeSlots = new Dictionary<Type, List<int>>();
            var typeBins = new Dictionary<Type, List<Card>>();
            var countedPoolReq = (InCountedPool<Type>)typeSampler.requirements.First(r => r is InCountedPool<Type>);

            log.LogDebug(string.Join(";", countedPoolReq.countedPool.Select(kv => $"{kv.Key.Name}:{kv.Value}")));

            for (int i = 0; i < toShuffle.Count; i++)
            {
                var card = toShuffle[i];
                var cType = card.GetType();
                typeBins.GetOrCreateVal(cType, () => new List<Card>(), out var cards);
                cards.Add(card);

                var rolledType = typeSampler.Roll(slotRng, null, out var samplerLogInfo);
                log.LogDebug(rolledType?.Name);
                log.LogDebug(samplerLogInfo.rolls);


                typeSlots.GetOrCreateVal(rolledType, () => new List<int>(), out var ctIndexes);
                ctIndexes.Add(i);
                countedPoolReq.ReduceCount(rolledType);
            }

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
