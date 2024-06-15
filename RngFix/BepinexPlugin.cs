using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using RngFix.CustomRngs;
using RngFix.Patches.Debug;
using System.Reflection;
using UnityEngine;


namespace RngFix
{
    [BepInPlugin(RngFix.PInfo.GUID, RngFix.PInfo.Name, RngFix.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = RngFix.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        // add this for audio loading
        internal static DirectorySource directorySource = new DirectorySource(RngFix.PInfo.GUID, "");


        public static ConfigEntry<bool> ignoreFactorsTableConf;

        public static ConfigEntry<bool> doLoggingConf;




        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            EntityManager.RegisterSelf();

            ignoreFactorsTableConf = Config.Bind("Rng", "IgnoreFactorsTable", true, "Disables the mechanic where card chance of appearing as a card reward is decreased if an offered card is not picked. Setting this to true greatly increases card reward consistency.");

            doLoggingConf = Config.Bind("Stats", "DoLogging", false, "Log card, exhibit roll results to csv files (WIP, Experimental)");

            harmony.PatchAll();

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
                WatermarkWrapper.ActivateWatermark();


            new RngSaveContainer().RegisterSelf(PInfo.GUID);
            PickCardLog.RegisterOnCardsAdded();
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


    }
}
