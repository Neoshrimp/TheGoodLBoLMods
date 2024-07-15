using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
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
    // 2do yachie
    // problems - sample size affect rng advance calls? +
    // 2do Aya not in the pool inconsistencies (same problem)
    // 2do in-battle manipulations?
    // 2do enemy move manip
    // 2do FallbackShinning uses ExhibitRng (doesn't really matter)
    // 2do transform card double roll (never happens in practice)
    public class GrRngs
    {
        static ConditionalWeakTable<GameRunController, GrRngs> table = new ConditionalWeakTable<GameRunController, GrRngs>();

        // persistent rngs
        public class PersRngs
        {
            public StageMasterRng stageMasterRng; // => eventRng, eliteRng
            public ActMasterRng actMasterRng; // => battleQueue

            // node
            public ulong prevNodeMasterState;
            // act
            public RandomGen enemyActQueueRng;
            public RandomGen battleInitRng;
            // stage
            public RandomGen eliteQueueRng;
            public ulong adventureQueueSeed;
            //public RandomGen adventureQueueRng;
            public RandomGen bossInitRng;
            public RandomGen eliteInitRng;
            //public RandomGen adventureInitRng;
            public RandomGen transitionInitRng;
            // run
            public RandomGen gapInitRng;
            public RandomGen shopInitRng;
            public RandomGen rareExhibitQueueRng;
            public RandomGen qingeUpgradeQueueRng;
            public RandomGen cardUpgradeQueueRng;

            public RandomGen extraCardRewardRng;

            public RandomGen eliteCardRng;
            public RandomGen bossCardRng;


            public RandomGen fallbackInitRng;
            public EntitySelfRngs<Exhibit> exhibitSelfRngs;
            public EntitySelfRngs<Adventure> adventureSelfRngs;
            public ShopRngs shopRngs;

        }

        public PersRngs persRngs = new PersRngs();


        public static RandomGen GetEnemyActQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.enemyActQueueRng;
        public static RandomGen GetEliteQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.eliteQueueRng;
        public static RandomGen GetAdventureQueueRng(GameRunController gr) => new RandomGen(GetOrCreate(gr).persRngs.adventureQueueSeed);
        public static RandomGen GetQingeUpgradeQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.qingeUpgradeQueueRng;
        public static RandomGen GetCardUpgradeQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.cardUpgradeQueueRng;

        public static RandomGen GetEliteCardRng(GameRunController gr) => GetOrCreate(gr).persRngs.eliteCardRng;

        private NodeMasterRng nodeMaster;

        public RandomGen transitionRng;
        public static RandomGen GetTransitionRng(GameRunController gr) => GetOrCreate(gr).transitionRng;
        public RandomGen battleLootRng;
        public RandomGen bossCardRewardRng; // unused
        public RandomGen moneyRewardRng;
        public RandomGen overdraftRng;

        public static RandomGen GetMoneyRewardRng(GameRunController gr) => GetOrCreate(gr).moneyRewardRng;


        public static RandomGen GetShopExRng(GameRunController gr) => GetOrCreate(gr).persRngs.shopRngs.exRng;


        // to keep nodeRng advancements consistent in-case of future requirements
        public RandomGen unusedNode0;
        public RandomGen unusedNode1;
        public RandomGen unusedNode2;
        public RandomGen unusedNode3;

        public RandomGen unusedRoot0;


        private Lazy<AbstractSlotSampler<Exhibit, Type>> normalExSampler = new Lazy<AbstractSlotSampler<Exhibit, Type>>(() => 
        new WeightedSlotSampler<Exhibit>(
            requirements: new List<ISlotRequirement<Type>>() { new ExInPool(), new ExHasManaColour() },
            initAction: (t) => { var ex = Library.CreateExhibit(t); Gr().ExhibitPool.Remove(ex.GetType()); return ex; },
            successAction: null,
            failureAction: null,
            potentialPool: Padding.ExPadding())
        );
        public AbstractSlotSampler<Exhibit, Type> NormalExSampler { get => normalExSampler.Value; }


        private Lazy<WeightedSlotSampler<Card>> cardSampler = new Lazy<WeightedSlotSampler<Card>>(() => 
        new WeightedSlotSampler<Card>(
            requirements: new List<ISlotRequirement<Type>>() { new CardInPool() },
            initAction: (t) => { var c = Library.CreateCard(t); c.GameRun = GrRngs.Gr(); return c; },
            successAction: null,
            failureAction: () => log.LogDebug("deeznuts"),
            potentialPool: Padding.RewardCards)
        );


        /*        private Lazy<SlotSampler<Card>> cardSampler = new Lazy<SlotSampler<Card>>(() => new SlotSampler<Card>(
                    requirements: new List<ISlotRequirement>() { new CardInPool() },
                    initAction: (t) => { var c = Library.CreateCard(t); c.GameRun = Gr(); return c; },
                    successAction: null,
                    failureAction: () => log.LogDebug("deeznuts"),
                    potentialPool: CardConfig.AllConfig().Where(cc => cc.IsPooled && cc.DebugLevel <= Gr().CardValidDebugLevel).Select(cc => TypeFactory<Card>.TryGetType(cc.Id)).Where(t => t != null).ToList()));*/


        public WeightedSlotSampler<Card> CardSampler { get => cardSampler.Value; }


        private Lazy<AbstractSlotSampler<Type, Type>> adventureSampler = new Lazy<AbstractSlotSampler<Type, Type>>(() => 
        new WeightedSlotSampler<Type>(
            requirements: new List<ISlotRequirement<Type>>() { new AdventureInPool(), new AdventureNOTinHistory() },
            initAction: (t) => { return t; },
            successAction: null,
            failureAction: null,
            potentialPool: Padding.AdventurePadding())
        );
        public AbstractSlotSampler<Type, Type> AdventureSampler { get => adventureSampler.Value; }

        public static RandomGen GetBossCardRng(GameRunController gr) => GetOrCreate(gr).persRngs.bossCardRng;

        public NodeMasterRng NodeMaster { get => nodeMaster; set => nodeMaster = value; }

        public EntitySelfRngs<Exhibit> ExhibitSelfRngs { get => persRngs.exhibitSelfRngs; set => persRngs.exhibitSelfRngs = value; }

        /// <summary>
        /// If a node contains several events of the same kind, StS arena style, i.e. adventure => battle => battle => adventure, NodeMaster should be advanced after each of those events to prevent rng bleedthrough between them.
        /// </summary>
        /// <param name=""></param>
        public void AdvaceNodeMaster(GameRunController gr)
        {
            NodeMaster.Advance(gr);
        }

        // 2do test
        /// <summary>
        /// Use this if skipping nodes by jumping horizontally. In vanilla, only Doremy event does this.
        /// </summary>
        /// <param name="gr"></param>
        /// <param name="jumpToNode"></param>
        /// <param name="suppressWarning"></param>
        public static void AdvanceRngsOnJump(GameRunController gr, MapNode jumpToNode, bool suppressWarning = false)
        {
            var cStation = gr.CurrentMap.VisitingNode;
            if (jumpToNode.X < cStation.X)
            {
                if (!suppressWarning)
                    log.LogWarning("Jumping to a previous level or act. Rngs cannot be reversed, seed consistency will be lost.");
                return;
            }

            var grRngs = GetOrCreate(gr);
            var steps = jumpToNode.Act - cStation.Act - (Helpers.IsActTransition(jumpToNode) ? 1 : 0);

            grRngs.persRngs.actMasterRng.AdvanceSteps(gr, steps);
            // makes sure current and future transition stations receive the same rng state as if they would have been reached without teleporting
            for (int i = 0; i < steps; i++)
                grRngs.persRngs.transitionInitRng.NextULong();


        }

        public static GameRunController Gr() => GameMaster.Instance?.CurrentGameRun;


        static public GrRngs GetOrCreate(GameRunController gr)
        {
            var grRgns = table.GetOrCreateValue(gr);
            return grRgns;
        }
    }
}
