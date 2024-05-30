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
    // onlyRare exhibit rolls
    // 2do lightBulb
    // 2do on exhibitGain rng
    // 2do test Doremy jump
    // 2do mana base wildly affects card reward (same state, different pool problem)
    // 2do Aya not in the pool inconsistencies (same problem)
    // 2do in-battle manipulations?
    public class GrRngs
    {
        static ConditionalWeakTable<GameRunController, GrRngs> table = new ConditionalWeakTable<GameRunController, GrRngs>();

        // persistent rngs
        public class PersRngs
        {
            public StageMasterRng stageMasterRng; // => eventRng, eliteRng
            public ActMasterRng actMasterRng; // => battleQueue

            //act
            public RandomGen enemyActQueueRng;
            public RandomGen battleInitRng;
            // stage
            public RandomGen eliteQueueRng;
            public RandomGen eventQueueRng;
            public RandomGen bossInitRng;
            public RandomGen eliteInitRng;
            public RandomGen adventureInitRng;
            public RandomGen transitionInitRng;
            // run
            public RandomGen gapInitRng;
            public RandomGen shopInitRng;
            public RandomGen rareExhibitQueueRng;

            public RandomGen fallbackInitRng;


        }

        public PersRngs persRngs = new PersRngs();


        public static RandomGen GetEnemyActQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.enemyActQueueRng;
        public static RandomGen GetEliteQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.eliteQueueRng;
        public static RandomGen GetEventQueueRng(GameRunController gr) => GetOrCreate(gr).persRngs.eventQueueRng;



        private NodeMasterRng nodeMaster;

        public RandomGen transitionRng;
        public static RandomGen GetTransitionRng(GameRunController gr) => GetOrCreate(gr).transitionRng;
        public RandomGen battleLootRng;
        public RandomGen bossCardRewardRng;
        public static RandomGen GetBoobossCardRewardRng(GameRunController gr) => GetOrCreate(gr).bossCardRewardRng;

        public NodeMasterRng NodeMaster { get => nodeMaster; set => nodeMaster = value; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        public void AdvaceNodeMaster(GameRunController gr)
        {
            NodeMaster.Advance(gr);
        }


        /// <summary>
        /// deeznuts
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
            log.LogDebug($"c: {cStation.X}, jumpTo:{jumpToNode.X}");

            var grRngs = GetOrCreate(gr);
            var steps = jumpToNode.Act - cStation.Act - (Helpers.IsActTransition(jumpToNode) ? 1 : 0);

            grRngs.persRngs.actMasterRng.AdvanceSteps(gr, steps);
            // makes sure current and future transition stations receive the same rng state as if they would have been reached without teleporting
            // in reality transitionInitRng isn't used for much
            for (int i = 0; i < steps; i++)
                GetTransitionRng(gr).NextULong();


        }


        static public GrRngs GetOrCreate(GameRunController gr)
        {
            var grRgns = table.GetOrCreateValue(gr);
            return grRgns;
        }
    }
}
