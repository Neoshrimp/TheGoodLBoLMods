using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Presentation;
using LBoLEntitySideloader.CustomHandlers;
using Logging;
using RngFix.CustomRngs;
using RngFix.CustomRngs.Sampling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace RngFix.Patches.Debug
{
    public class StatsLogger
    {

        public static string[] generalHead = new string[] { "ManaBase", "Char", "Exhibits" };

        public static string[] cardsHeader = new string[] { "Card", "Rarity", "Colors", "RareFactorAfter", "CardRng", "ShopRng", "UpgradeRng"};

        public static string[] addedCardHead = new string[] { "AddedCard", "Amount", "Rarity", "Colors" };

        public static List<KeyValuePair<string, MethodInfo>> vanillaRngsHead = new List<KeyValuePair<string, MethodInfo>>();
        public static List<KeyValuePair<string, FieldInfo>> persistentRngsHead = new List<KeyValuePair<string, FieldInfo>>();

        public static string[] exHeader = new string[] { "Exhibit", "Rarity", "Pool", "ExRng", "ShopRng", "AdvRng" };

        public static string[] eventHead = new string[] { "Adventure", "AdvInitRng" };


        public static string[] rollHead = new string[] { "ItemW", "WThreshold", "MaxW", "TotalW", "Rolls" };
        public static string[] commonHead = new string[] { "Station", "Event", "Stage", "Act", "X", "Y" };

        static MethodInfo mi_rngState = AccessTools.PropertyGetter(typeof(RandomGen), nameof(RandomGen.State));

        public static void LogGeneral(GameRunController gr)
        {
            var log = GetGeneralLog(gr);
            log.SetValSafe(gr.BaseMana, "ManaBase");
            log.SetValSafe(gr.Player.Name, "Char");
            log.SetValSafe(string.Join("|", gr.Player.Exhibits), "Exhibits");

            LogCommonAndFlush(log, gr);
        }

        public static void LogEvent(Type adv, GameRunController gr, SamplerLogInfo li)
        {
            var log = GetEventLog(gr);
            var grRngs = GrRngs.GetOrCreate(gr);

            log.SetValSafe(adv.Name, "Adventure");
            log.SetValSafe(grRngs.persRngs.adventureInitRng.State, "AdvInitRng");

            LogRoll(log, li);
            LogCommonAndFlush(log, gr);
        }


        public static void LogEx(Exhibit ex, GameRunController gr, SamplerLogInfo li)
        {
            var log = GetExLog(gr);
            var grRngs = GrRngs.GetOrCreate(gr);

            log.SetValSafe(ex?.Name, "Exhibit");
            log.SetValSafe(ex?.Config.Rarity, "Rarity");
            log.SetValSafe(ex?.Config.Appearance, "Pool");
            log.SetValSafe(gr.ExhibitRng.State, "ExRng");
            log.SetValSafe(gr.ShopRng.State, "ShopRng");
            log.SetValSafe(gr.AdventureRng.State, "AdvRng");

            LogRoll(log, li);
            LogCommonAndFlush(log, gr);
        }

        public static void LogVanillaRngs(GameRunController gr)
        {
            var log = GetVanillaRngsLog(gr);
            foreach (var kv in vanillaRngsHead)
            {
                var rng = kv.Value.Invoke(gr, Array.Empty<object>());
                var state = 0ul;
                if (rng != null)
                    state = (ulong)mi_rngState.Invoke(rng, Array.Empty<object>());
                log.SetValSafe(state, kv.Key);
            }
            LogCommonAndFlush(log, gr);

        }

        public static void LogPersistentRngs(GameRunController gr)
        {
            var log = GetPersistentRngsLog(gr);
            var grRngs = GrRngs.GetOrCreate(gr);

            foreach (var kv in persistentRngsHead)
            {
                var rng = kv.Value.GetValue(grRngs.persRngs);
                var state = 0ul;
                if (rng != null)
                    state = (ulong)mi_rngState.Invoke(rng, Array.Empty<object>());
                log.SetValSafe(state, kv.Key);
            }
            LogCommonAndFlush(log, gr);
        }


        public static void LogCard(Card card, GameRunController gr, SamplerLogInfo li)
        {
            var log = GetCardLog(gr);
            var grRngs = GrRngs.GetOrCreate(gr);

            log.SetValSafe(card?.Name, "Card");
            log.SetValSafe(card?.Config.Rarity, "Rarity");
            if(card != null)
                log.SetValSafe(string.Join("", card?.Config.Colors), "Colors");     
            log.SetValSafe(gr._cardRareWeightFactor, "RareFactorAfter");
            log.SetValSafe(gr.CardRng.State, "CardRng");
            log.SetValSafe(gr.ShopRng.State, "ShopRng");
            log.SetValSafe(grRngs.persRngs.cardUpgradeQueueRng?.State, "UpgradeRng");

            LogRoll(log, li);
            LogCommonAndFlush(log, gr);
        }

        public static void LogPickedCard(Card card, int amount, GameRunController gr)
        {
            var log = GetPickedCardLog(gr);

            log.SetValSafe(card?.Name, "AddedCard");
            log.SetValSafe(amount, "Amount");
            log.SetValSafe(card?.Config.Rarity, "Rarity");
            if(card != null)
                log.SetValSafe(string.Join("", card?.Config.Colors), "Colors");         
            LogCommonAndFlush(log, gr);
        }

        public static void LogRoll(CsvLogger logger, SamplerLogInfo li)
        {
            logger.SetValSafe(li.itemW, "ItemW");
            logger.SetValSafe(li.wThreshold, "WThreshold");
            logger.SetValSafe(li.totalW, "TotalW");
            logger.SetValSafe(li.maxW, "MaxW");
            logger.SetValSafe(li.rolls, "Rolls");
        }

        public static void LogCommonAndFlush(CsvLogger logger, GameRunController gr, bool flush = true)
        {
            logger.SetValSafe(gr.CurrentMap.VisitingNode.StationType, "Station");
            logger.SetValSafe(gr.CurrentMap.VisitingNode.Act, "Act");
            logger.SetValSafe(gr.CurrentStage.Name, "Stage");
            logger.SetValSafe(gr.CurrentMap.VisitingNode.X, "X");
            logger.SetValSafe(gr.CurrentMap.VisitingNode.Y, "Y");

            if (gr.CurrentStation is AdventureStation adventureStation)
            {
                logger.SetValSafe("adventure:" + adventureStation.Adventure.HostName, "Event");
            }
            else if (gr.CurrentStation is BattleStation bs)
            {
                logger.SetValSafe("battle:" + bs.EnemyGroup.Id, "Event");
            }
            else if (gr.CurrentStation is EliteEnemyStation es)
            {
                logger.SetValSafe("elite:" + es.EnemyGroup.Id, "Event");
            }
            else if (gr.CurrentStation is BossStation bos)
            {
                logger.SetValSafe("boss:" + bos.EnemyGroup.Id, "Event");
            }

            if(flush)
                logger.FlushVals();
        }


        private static CsvLogger GetLog(GameRunController gr, string prefix ="", string ext = ".csv", bool isEnabled = true)
        {
            var ss = RandomGen.SeedToString(gr.RootSeed);
            var logId = prefix + "_" + ss;
            var logger = CsvLogger.GetOrCreateLog(
                logFile: logId,
                ass: gr,
                ext: ext,
                subFolder: "RngFix",
                isEnabled: isEnabled);
            return logger;
        }

        public static CsvLogger GetCardLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, "cards", isEnabled: isEnabled);
        public static CsvLogger GetGeneralLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, "general", isEnabled: isEnabled);
        public static CsvLogger GetPickedCardLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, "pickedCards", isEnabled: isEnabled);
        public static CsvLogger GetExLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, prefix:"ex", isEnabled: isEnabled);
        public static CsvLogger GetEventLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, prefix: "event", isEnabled: isEnabled);
        public static CsvLogger GetVanillaRngsLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, prefix: "vanillaRngs", isEnabled: isEnabled);
        public static CsvLogger GetPersistentRngsLog(GameRunController gr, bool isEnabled = true) => GetLog(gr, prefix: "persistentRngs", isEnabled: isEnabled);



        public static GameRunController Gr() => GameMaster.Instance?.CurrentGameRun;


        [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
        class InitLog
        {
            static void Postfix(GameRunController __result)
            {
                var gr = __result;


                var doLog = BepinexPlugin.doLoggingConf.Value;


                GetCardLog(gr, doLog).SetHeader(cardsHeader.Concat(rollHead).Concat(commonHead));
                GetCardLog(gr).LogHead();

                GetGeneralLog(gr, doLog).SetHeader(generalHead.Concat(commonHead));
                GetGeneralLog(gr).LogHead();

                GetPickedCardLog(gr, doLog).SetHeader(addedCardHead.Concat(commonHead));
                GetPickedCardLog(gr).LogHead();

                GetExLog(gr, doLog).SetHeader(exHeader.Concat(rollHead).Concat(commonHead));
                GetExLog(gr).LogHead();

                GetEventLog(gr, doLog).SetHeader(eventHead.Concat(rollHead).Concat(commonHead));
                GetEventLog(gr).LogHead();


                vanillaRngsHead.Clear();
                GetVanillaRngsLog(gr, doLog).SetHeader(AccessTools.GetDeclaredProperties(typeof(GameRunController))
                    .Where(p => p.GetMethod?.Name.EndsWith("Rng") ?? false)
                    .Select(p => { vanillaRngsHead.Add(new KeyValuePair<string, MethodInfo>(p.Name, p.GetMethod)); return p.Name; })
                    .Concat(commonHead));
                GetVanillaRngsLog(gr).LogHead();

                persistentRngsHead.Clear();
                GetPersistentRngsLog(gr, doLog).SetHeader(AccessTools.GetDeclaredFields(typeof(GrRngs.PersRngs))
                    .Where(f => f.FieldType == typeof(RandomGen))
                    .Select(f => { persistentRngsHead.Add(new KeyValuePair<string, FieldInfo>(f.Name, f)); return f.Name; })
                    .Concat(commonHead));
                GetPersistentRngsLog(gr).LogHead();

            }
        }
    }


    public static class CsvLoggerExt
    {
        public static void SetValSafe(this CsvLogger logger, object val, string valKey)
        {
            try
            {
                logger.SetVal(val, valKey, true);
            }
            catch (Exception ex)
            {
                BepinexPlugin.log.LogWarning(logger.logFile + ":" + ex);
            }
        }
    }

}