using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Neutral.Blue;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.EntityLib.Cards.Neutral.Red;
using LBoL.EntityLib.Cards.Neutral.White;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Resource;
using RngFix.CustomRngs;
using RngFix.CustomRngs.Sampling;
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.CustomRngs.Sampling.UniformPools;
using RngFix.Patches.Cards;
using RngFix.Patches.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;


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

        public static ConfigEntry<bool> disableManaBaseAffectedCardWeights;




        private void Awake()
        {

            log = Logger;

            // very important. Without this the entry point MonoBehaviour gets destroyed
            DontDestroyOnLoad(gameObject);
            gameObject.hideFlags = HideFlags.HideAndDontSave;

            EntityManager.RegisterSelf();

            ignoreFactorsTableConf = Config.Bind("Rng", "IgnoreFactorsTable", true, "Disables the mechanic where card chance of appearing as a card reward is decreased if an offered card is not picked. Setting this to true greatly increases card reward consistency.");

            doLoggingConf = Config.Bind("Stats", "DoLogging", true, "Log various run stats (card, exhibits etc.) to <gameSaveDir>/RngFix");

            disableManaBaseAffectedCardWeights = Config.Bind("Rng", "disableManaBaseAffectedCardWeights", false, "In vanilla, card weights are influenced by current mana. Roughly the more of a certain colour there is in the mana base the more likely cards of that colour are to appear. This disrupts seed consistency somewhat. But disabling this mechanic by default is potentially a big enough deviation from vanilla experience that an optional toggle is justified.");

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


        //[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.Restore))]
        class GrRestore_Patch
        {
            static void Postfix(GameRunController __result)
            {
                log.LogDebug("restoredeez");
                __result.CardValidDebugLevel = 0;
            }
        }







        KeyboardShortcut debgugBind = new KeyboardShortcut(KeyCode.Y, new KeyCode[] { KeyCode.LeftShift });

        private void Update()
        {
            if (false && debgugBind.IsDown() && GrRngs.Gr() != null)
            {
                var gr = GrRngs.Gr();
                var grrngs = GrRngs.GetOrCreate(gr);
                var stage = gr.CurrentStage;
                //DisableManaBaseAffectedCardWeights_Patch.tempDebugDisable = false;



                var seed = RandomGen.ParseSeed("deeznuts");
                log.LogDebug(string.Join(";", BattleRngs.Shuffle(new RandomGen(seed),
                    new List<Card>() {
                        Library.CreateCard<Shoot>(),
                        Library.CreateCard<Shoot>(),

                        Library.CreateCard<SuikaBigball>(),
                        Library.CreateCard<IceBarrier>(),
                        Library.CreateCard<FrostGarden>(),
                        //Library.CreateCard<Shoot>(),

                        Library.CreateCard<MeilingBlock>(),
                        Library.CreateCard<PerfectServant>(),
                        Library.CreateCard<TwoBalls>(),
                        //Library.CreateCard<TwoBalls>(),
                }).Select(c => $"{c.Name}")));


                log.LogDebug(string.Join(";", BattleRngs.Shuffle(new RandomGen(seed),
                    new List<Card>() {
                        Library.CreateCard<Shoot>(),
                        Library.CreateCard<Shoot>(),

                        Library.CreateCard<SuikaBigball>(),
                        Library.CreateCard<IceBarrier>(),
                        Library.CreateCard<FrostGarden>(),
                        //Library.CreateCard<Shoot>(),

                        Library.CreateCard<MeilingBlock>(),
                        //Library.CreateCard<MeilingBlock>(),

                        Library.CreateCard<PerfectServant>(),
                        //Library.CreateCard<PerfectServant>(),

                        Library.CreateCard<TwoBalls>(),
                        Library.CreateCard<TwoBalls>(),
                }).Select(c => $"{c.Name}")));

                var bigDick = new List<Card>(Enumerable.Repeat<Card>(Library.CreateCard<Shoot>(), 9999));
                var bigRng = new RandomGen(RandomGen.GetRandomSeed());
                var sw = new Stopwatch();
                sw.Start();

                BattleRngs.Shuffle(new RandomGen(), bigDick);

                sw.Stop();
                log.LogDebug(sw.Elapsed);

                /*                gr.CardValidDebugLevel = 0;
                                grrngs.CardSampler.BuildPool(Padding.CardPadding());
                                SamplerDebug.RollDistribution(GameMaster.Instance.CurrentGameRun.CurrentStage.DrinkTeaAdditionalCardWeight, SamplerDebug.SamplingMethod.Slot, battleRolling: false, rolls: 1000, seed: 2405181760243075183);

                                gr.CardValidDebugLevel = 1;
                                grrngs.CardSampler.BuildPool(Padding.CardPadding());
                                SamplerDebug.RollDistribution(GameMaster.Instance.CurrentGameRun.CurrentStage.DrinkTeaAdditionalCardWeight, SamplerDebug.SamplingMethod.Slot, battleRolling: false, rolls: 1000, seed: 2405181760243075183)*/
                


                /*                var cChar = SamplerDebug.SimulateCardRoll(9264767910880677932, stage.BossCardCharaWeight, out SamplerLogInfo _, manaBase: new ManaGroup() { Green = 3, Blue = 2, Colorless = 1 }, sampler: grrngs.CardSampler, logToFile: true);
                                var cFriend = SamplerDebug.SimulateCardRoll(9264767910880677932, stage.BossCardFriendWeight, out SamplerLogInfo _, manaBase: new ManaGroup() { Green = 3, Blue = 2, Colorless = 1 }, sampler: grrngs.CardSampler, logToFile: true);
                                var cNeutral = SamplerDebug.SimulateCardRoll(9264767910880677932, stage.BossCardNeutralWeight, out SamplerLogInfo _, manaBase: new ManaGroup() { Green = 3, Blue = 2, Colorless = 1 }, sampler: grrngs.CardSampler, logToFile: true);

                                log.LogDebug(cChar);
                                log.LogDebug(cFriend);
                                log.LogDebug(cNeutral);*/


                /*                gr._cardRareWeightFactor = 0.9f;
                                gr._cardRewardDecreaseRepeatRare = true;
                                var cards = gr.GetRewardCards(stage.BossCardCharaWeight, stage.BossCardFriendWeight, stage.BossCardNeutralWeight, stage.BossCardWeight,3, true);
                                log.LogDebug(string.Join<Card>(", ", cards));*/





                //SamplerDebug.RollDistribution(stage.DrinkTeaAdditionalCardWeight, SamplerDebug.SamplingMethod.EwSlot, battleRolling: false, rolls: 100, seed: null, manaBase: new ManaGroup() { White = 2, Blue = 3, Black = 0 }, filter: t => t.Name == nameof(YonglinCard) || t.Name == nameof(HuiyeMarblePhantasm));

                /*                var rangeSet = new RangeCollection();
                var range1 = new ManRange(1, 10);
                rangeSet.Add(range1);
                rangeSet.Add(4, 15);

                rangeSet.Add(1, 3);

                rangeSet.Add(16, 19);
                //rangeSet.Remove(new ManRange(20, 25));
                log.LogDebug(rangeSet);
                log.LogDebug(rangeSet.Count);*/






                //SamplerDebug.RollDistribution(GameMaster.Instance.CurrentGameRun.CurrentStage.DrinkTeaAdditionalCardWeight, SamplerDebug.SamplingMethod.Vanilla, battleRolling: false, rolls: 1000, seed: 12012204824104114439, manaBase: new ManaGroup() { White = 2, Blue = 3, Black = 0 });


                //SamplerDebug.RollDistribution(GameMaster.Instance.CurrentGameRun.CurrentStage.DrinkTeaAdditionalCardWeight, SamplerDebug.SamplingMethod.EwSlot, battleRolling: false, rolls: 1000, seed: 4627015065581599883, manaBase: new ManaGroup() { White = 2, Blue = 2, Black = 1 });


                /*                var sampler = SamplerDebug._ewSampler.Value;
                                var seed = RandomGen.FromState(1062950951210960947).NextULong();
                                sampler.extraRolls = 0;
                                SamplerDebug.SimulateCardRoll(seed, GameMaster.Instance.CurrentGameRun.CurrentStage.DrinkTeaAdditionalCardWeight, out SamplerLogInfo _, manaBase: new ManaGroup() { White = 2, Blue = 3, Black = 0 }, sampler: sampler, logToFile: true);
                                sampler.extraRolls = 0;

                                sampler.extraRolls = 0;
                                SamplerDebug.SimulateCardRoll(seed, GameMaster.Instance.CurrentGameRun.CurrentStage.DrinkTeaAdditionalCardWeight, out SamplerLogInfo _, manaBase: new ManaGroup() { White = 2, Blue = 2, Black = 1 }, sampler: sampler, logToFile: true);
                                sampler.extraRolls = 0;*/
            }
        }

    }


}
