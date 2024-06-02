using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Stations;
using RngFix.Patches;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.CustomRngs
{
    // problems - sample size affect rng advance calls? +
    // 2do mana base wildly affects card reward (same state, different pool problem)
    // 2do Aya not in the pool inconsistencies (same problem)
    // 2do in-battle manipulations?
    // 2do event pools are act (a tiny bit) interdependent
    // 2do FallbackShinning uses ExhibitRng (doesn't really matter)
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
            public RandomGen adventureQueueRng;
            public RandomGen bossInitRng;
            public RandomGen eliteInitRng;
            public RandomGen adventureInitRng;
            public RandomGen transitionInitRng;
            // run
            public RandomGen gapInitRng;
            public RandomGen shopInitRng;
            public RandomGen rareExhibitQueueRng;
            public RandomGen upgradeQueueRng;

            public RandomGen fallbackInitRng;
            public ExhibitSelfRngs exhibitSelfRngs;

        }

        public PersRngs persRngs = new PersRngs();


        public static RandomGen GetEnemyActQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.enemyActQueueRng;
        public static RandomGen GetEliteQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.eliteQueueRng;
        public static RandomGen GetAdventureQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.adventureQueueRng;
        public static RandomGen GetUpgradeQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.upgradeQueueRng;

        public NodeMasterRng nodeMaster;

        public RandomGen transitionRng;
        public static RandomGen GetTransitionRng(GameRunController gr) => GetOrCreate(gr).transitionRng;
        public RandomGen battleLootRng;
        public RandomGen bossCardRewardRng;
        public RandomGen moneyRewardRng;
        public static RandomGen GetMoneyRewardRng(GameRunController gr) => GetOrCreate(gr).moneyRewardRng;



        // to keep nodeRng advancements consistent in-case of future requirements
        public RandomGen unusedNode0;
        public RandomGen unusedNode1;
        public RandomGen unusedNode2;
        public RandomGen unusedNode3;
        public RandomGen unusedNode4;

        public RandomGen unusedRoot0;
        public RandomGen unusedRoot1;
        public RandomGen unusedRoot2;
        public RandomGen unusedRoot3;
        public RandomGen unusedRoot4;




        public static RandomGen GetBossCardRewardRng(GameRunController gr) => GetOrCreate(gr).bossCardRewardRng;

        public NodeMasterRng NodeMaster { get => nodeMaster; set => nodeMaster = value; }

        public ExhibitSelfRngs ExhibitSelfRngs { get => persRngs.exhibitSelfRngs; set => persRngs.exhibitSelfRngs = value; }

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
                if(!suppressWarning)
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


        static public GrRngs GetOrCreate(GameRunController gr)
        {
            var grRgns = table.GetOrCreateValue(gr);
            return grRgns;
        }
    }
}
