using BepInEx;
using HarmonyLib;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System.Reflection;
using UnityEngine;
using VariantsC.Rng;

namespace VariantsC
{
    [BepInPlugin(VariantsC.PInfo.GUID, VariantsC.PInfo.Name, VariantsC.PInfo.version)]
    [BepInDependency(LBoLEntitySideloader.PluginInfo.GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(AddWatermark.API.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LBoL.exe")]
    public class BepinexPlugin : BaseUnityPlugin
    {

        private static readonly Harmony harmony = VariantsC.PInfo.harmony;

        internal static BepInEx.Logging.ManualLogSource log;

        internal static TemplateSequenceTable sequenceTable = new TemplateSequenceTable();

        internal static IResourceSource embeddedSource = new EmbeddedSource(Assembly.GetExecutingAssembly());

        // add this for audio loading
        internal static DirectorySource directorySource = new DirectorySource(VariantsC.PInfo.GUID, "resources");

        internal static BepInEx.Configuration.ConfigEntry<bool> poolNewCards;

        internal static BepInEx.Configuration.ConfigEntry<bool> poolNewExhibits;


        internal static BatchLocalization CardBatchLoc = new BatchLocalization(directorySource, typeof(CardTemplate), "Card");
        internal static BatchLocalization StatusEffectBatchLoc = new BatchLocalization(directorySource, typeof(StatusEffectTemplate), "StatusEffect");
        internal static BatchLocalization ExhibitBatchLoc = new BatchLocalization(directorySource, typeof(ExhibitTemplate), "Exhibit");
        internal static BatchLocalization UltimateSkillBatchLoc = new BatchLocalization(directorySource, typeof(UltimateSkillTemplate), "UltimateSkill");


        private void Awake()
        {
            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            poolNewCards = Config.Bind("Pools", "poolNewCardsWhenExhibit", false, "Pool new starting cards of a loadout when not in a possession of the corresponding loadout exhibit. For example, when set to false, Blood Magic and Consequence of Hickeys cards won't be rewarded or generated unless player has Bloody Ripper exhibit.");

            poolNewExhibits = Config.Bind("Pools", "poolNewExhibits", false, "Makes C variant exhibits discoverable after defeating their owner.");

            new SaveContainer().RegisterSelf(PInfo.GUID);

            EntityManager.RegisterSelf();

            harmony.PatchAll();

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(AddWatermark.API.GUID))
                WatermarkWrapper.ActivateWatermark();


            CustomLoadouts.AddLoadouts();
        }

        private void OnDestroy()
        {
            if (harmony != null)
                harmony.UnpatchSelf();
        }


    }
}
