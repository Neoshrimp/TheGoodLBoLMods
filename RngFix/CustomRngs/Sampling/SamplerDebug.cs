using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using Logging;
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.Patches.Battle;
using RngFix.Patches.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.CustomRngs.Sampling
{
    public static class SamplerDebug
    {
        [HarmonyPatch]
        class RollCards_ReversePatch
        {
            [HarmonyReversePatch]
            [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollCards), new Type[] { typeof(RandomGen), typeof(CardWeightTable), typeof(int), typeof(ManaGroup?), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(Predicate<CardConfig>) })]
            public static Card[] RollCards(GameRunController instance, RandomGen rng, CardWeightTable weightTable, int count, ManaGroup? manaLimit, bool colorLimit, bool applyFactors = false, bool battleRolling = false, bool ensureCount = false, [MaybeNull] Predicate<CardConfig> filter = null)
            {
                return null;
            }
        }

        public static void TestInsert(ulong seed, int toInsert, int iterations = 10000, int deckCount = 50)
        {
            using var logger = new CsvLogger($"{DateTime.Now.Ticks % (int)1E8}_{seed}_{toInsert}insertInto{deckCount}", subFolder: "Rngfix");

            log.LogDebug("Insert test..");
            var rng = new RandomGen(seed);
            var posCount = new Dictionary<int, int>();

            for (int i = 0; i < iterations; i++)
            {
                var subRng = new RandomGen(rng.NextULong());
                for (int j = 0; j < toInsert; j++)
                {
                    var pos = AddCardToDrawZone_Patch.ConsistentDeckPos(subRng, 0, deckCount+j);
                    posCount.TryAdd(pos, 0);
                    posCount[pos]++;
                
                }
            }

            log.LogDebug($"total:{toInsert * iterations}");
            log.LogDebug(string.Join(";", posCount.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}:{kv.Value}")));
            logger.SetHeader(new string[] { "Pos", "Count" });
            logger.LogHead();
            posCount.OrderBy(kv => kv.Key).Do(kv => {
                logger.SetVal(kv.Key, "Pos");
                logger.SetVal(kv.Value, "Count");
                logger.FlushVals();
            });
        }

        public enum SamplingMethod 
        {
            Slot,
            Vanilla,
            EwSlot
        }

        public static string[] cardHead = new string[] { "Card", "Rarity", "Colors", "Rng" };

        public static Lazy<WeightedSlotSampler<Card>> _ewSampler = new Lazy<WeightedSlotSampler<Card>>(() => new WeightedSlotSampler<Card>(
            requirements: new List<ISlotRequirement<Type>>() { new CardInPool() },
            initAction: (t) => { var c = Library.CreateCard(t); c.GameRun = GrRngs.Gr(); return c; },
            successAction: null,
            failureAction: () => log.LogDebug("deeznuts"),
            potentialPool: Padding.RewardCards
            ));

        public static void RollDistribution(CardWeightTable weightTable, SamplingMethod samplingMethod = SamplingMethod.Slot, int rolls = 2000, bool battleRolling = false, ulong? seed = null, ManaGroup? manaBase = null, Predicate<Type> filter = null)
        {
            var gr = GrRngs.Gr();
            if (gr == null)
                return;
            var grRgns = GrRngs.GetOrCreate(gr);
            if (seed == null)
                seed = RandomGen.GetRandomSeed();
            log.LogDebug($"Distribution: {seed},");

            manaBase ??= gr.BaseMana;
            var rng = new RandomGen(seed.Value);
            using (var logger = new CsvLogger($"{DateTime.Now.Ticks%(int)1E8}_{seed}_{samplingMethod}_{manaBase}", subFolder: "Rngfix"))
            {
                var charExSet = new HashSet<string>(gr.Player.Exhibits.Where(e => e.OwnerId != null).Select(e => e.OwnerId));
                logger.SetHeader(cardHead.Concat(StatsLogger.rollHead));
                logger.SetCollumnToSanitize("Card");
                logger.LogHead();

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                for (int i = 0; i < rolls; i++)
                {
                    Card card = null;
                    SamplerLogInfo logInfo = null;

                    logger.SetVal(rng.State, "Rng");

                    switch (samplingMethod)
                    {
                        case SamplingMethod.Slot:
                            card = SimulateCardRoll(
                                state: rng.NextULong(),
                                weightTable: weightTable,
                                logInfo: out logInfo,
                                sampler: grRgns.RewardCardSampler,
                                battleRolling: battleRolling,
                                manaBase: manaBase,
                                filter: filter,
                                doLogDebug: false);
                            break;
                        case SamplingMethod.Vanilla:
                            card = RollCards_ReversePatch.RollCards(
                                instance: gr,
                                rng: rng,
                                weightTable: weightTable,
                                count: 1,
                                manaLimit: manaBase,
                                colorLimit: true,
                                applyFactors: false,
                                battleRolling: battleRolling,
                                ensureCount: false,
                                filter: null
                                )[0];
                            break;
                        case SamplingMethod.EwSlot:
                            card = SimulateCardRoll(
                                state: rng.NextULong(),
                                weightTable: weightTable,
                                logInfo: out logInfo,
                                sampler: _ewSampler.Value,
                                battleRolling: battleRolling,
                                manaBase: manaBase,
                                filter: filter,
                                doLogDebug: false);
                            break;
                        default:
                            break;
                    }

                    if (card == null)
                        continue;

                    logger.SetVal(card.Name, "Card");
                    logger.SetVal(card.Config.Rarity, "Rarity");
                    logger.SetVal(string.Join("", card?.Config.Colors), "Colors");
                    if (logInfo != null)
                        StatsLogger.LogRoll(logger, logInfo);
                    else
                    {
                        CardConfig cc = card.Config;
                        var weight = weightTable.WeightFor(cc, gr.Player.Id, charExSet) * gr.BaseCardWeight(cc, false);
                        logger.SetVal(weight, "ItemW");
                    }
                    logger.FlushVals();
                }
                stopwatch.Stop();
                log.LogDebug($"{rolls} rolls in: {stopwatch.Elapsed.ToString("c")}");
            
            }

        }

        public static Card SimulateCardRoll(ulong state, CardWeightTable weightTable, out SamplerLogInfo logInfo, AbstractSlotSampler<Card, Type> sampler = null, bool battleRolling = false, ManaGroup? manaBase = null, Predicate<Type> filter = null, bool doLogDebug = false, bool logToFile = false)
        {
            var gr = GrRngs.Gr();
            logInfo = null;

            if (gr == null)
                return null;

            var grRgns = GrRngs.GetOrCreate(gr);
            if(sampler == null)
                sampler = grRgns.RewardCardSampler;
            var rng = RandomGen.FromState(state);

            var ogManabase = gr.BaseMana;
            if (manaBase != null)
                gr.BaseMana = manaBase.Value;
            else
                manaBase = gr.BaseMana;

            FileStream fileStream = null;
            StreamWriter streamWriter = null;
            if (logToFile)
            { 
                var dir = "RollDebug";
                Directory.CreateDirectory(dir);
                fileStream = File.Open($"{dir}/{state}_{manaBase}.txt", FileMode.Create, FileAccess.Write, FileShare.None);
                streamWriter = new StreamWriter(fileStream, Encoding.UTF8) { AutoFlush = true };
            }

            

            var cardPoolReq = sampler.requirements.Find(r => r is CardInPool) as CardInPool;

            cardPoolReq.poolSet = new HashSet<Type>(gr.CreateValidCardsPool(weightTable: weightTable, manaLimit: manaBase, colorLimit: gr.RewardAndShopCardColorLimitFlag == 0, applyFactors: false, battleRolling: battleRolling, filter: null).Select(re => re.Elem));

            var charExSet = new HashSet<string>(gr.Player.Exhibits.Where(e => e.OwnerId != null).Select(e => e.OwnerId));

            bool logWBreakdown = false;

            Func<Type, float> getW = t => {
                var cc = CardConfig.FromId(t.Name);
                var wt = weightTable.WeightFor(cc, gr.Player.Id, charExSet);
                var bw = gr.BaseCardWeight(cc, false);
/*                var toLog = $"weightT:{wt};baseW:{bw}";
                if (logWBreakdown && doLogDebug)
                    log.LogDebug(toLog);
                if (logWBreakdown && logToFile)
                    streamWriter.WriteLine(toLog);*/
                return wt * bw;
            };



            sampler.debugAction = (Li, t) => {
                logWBreakdown = true;
                var toLog = $"{Li.rolls} {t.Name}";
                if (doLogDebug)
                {
                    log.LogDebug(toLog);
                    log.LogDebug(Li);
                }
                if (logToFile)
                {
                    streamWriter.WriteLine(toLog);
                    streamWriter.WriteLine(Li);
                }
            };


            var card = sampler.Roll(rng, getW, out logInfo, filter);

            if(doLogDebug)
                log.LogDebug("Winning roll: " + logInfo);

            if (logToFile)
                streamWriter.WriteLine("Winning roll: " + logInfo);

            sampler.debugAction = null;

            gr.BaseMana = ogManabase;

            streamWriter?.Close();
            fileStream?.Close();

            return card;

        }
    }


}
