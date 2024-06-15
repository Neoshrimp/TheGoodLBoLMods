using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Presentation;
using LBoLEntitySideloader.CustomHandlers;
using Logging;
using System.Linq;
using System.Runtime.Serialization;

namespace RngFix.Patches.Debug
{
    public class StatsLogger
    {

        //public static string[] cardsHeader = new string[] { "Card1", "Card2", "Card3", "Card4", "RFbefore", "RFAfter", "CardRngAfter", "Station", "Picked" };

        public static string[] cardsHeader = new string[] { "Card", "Rarity", "Pool", "WeightWhenRolled", "TW", "RFAfter", "CardRngAfter", "Station" };


        public static string[] exHeader = new string[] { "Exhibit", "Rarity", "WeightWhenRolled", "TW", "ExRngStateAfter", "Station" };

        // stinky ass hacks
        private static CsvLogger GetLog(GameRunController gr, string prefix ="", string ext = ".csv", bool plusOne = false, int countMod = 0)
        {
            var ss = RandomGen.SeedToString(gr.RootSeed);
            var logId = prefix + ss + "_" + (CsvLogger.Count() + (plusOne ? 1 : 0) + countMod);
            var logger = CsvLogger.GetOrCreateLog(logId, ext, "RngFix");
            return logger;
        }

        public static CsvLogger GetCardLog(GameRunController gr, bool plusOne = false, int countMod = -1) => GetLog(gr, plusOne: plusOne, countMod: countMod);

        public static CsvLogger GetExLog(GameRunController gr, bool plusOne = false, int countMod = -1) => GetLog(gr, prefix:"ex_", plusOne: plusOne, countMod: countMod);

        public static GameRunController Gr() => GameMaster.Instance?.CurrentGameRun;
    }


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Create))]
    class InitLog
    {
        static void Postfix(GameRunController __result)
        {
            if (!BepinexPlugin.doLoggingConf.Value)
                return;
            var log = StatsLogger.GetCardLog(__result, plusOne: true, countMod: 0);
            log.Log(StatsLogger.cardsHeader);
            var exLog = StatsLogger.GetExLog(__result, plusOne: true);
            exLog.Log(StatsLogger.exHeader);
        }
    }
}