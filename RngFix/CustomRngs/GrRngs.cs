using LBoL.Base;
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
    // problems - sample size affect rng adavance calls?
    // 2do separate card rngs
    public class GrRngs
    {
        static ConditionalWeakTable<GameRunController, GrRngs> table = new ConditionalWeakTable<GameRunController, GrRngs>();

        public RandomGen rootNodeRng;
        public RandomGen rootActRng;

        public RandomGen enemyActRng;
        public static RandomGen GetEnemyActRng(GameRunController gr) => GetOrCreate(gr).enemyActRng;
        public RandomGen eliteActRng;
        public static RandomGen GetEliteActRng(GameRunController gr) => GetOrCreate(gr).eliteActRng;
        public RandomGen eventActRng;
        public static RandomGen GetEventActRng(GameRunController gr) => GetOrCreate(gr).eventActRng;

        // no need to save
        public RandomGen battleLootRng;


        public static void AssignActRngs(GameRunController gr, Func<RandomGen> rngProvider)
        {
            var grRngs = GetOrCreate(gr);

            grRngs.enemyActRng = rngProvider();
            grRngs.eliteActRng = rngProvider();
            grRngs.eventActRng = rngProvider();

            gr.AdventureRng = rngProvider();

        }

        public static void AssignNodeRngs(GameRunController gr, Func<RandomGen> rngProvider)
        {
            gr.GameRunEventRng = rngProvider(); // this still leaves some manipulation possible but w/e
            gr.BattleRng = rngProvider();
            gr.BattleCardRng = rngProvider();
            gr.ShuffleRng = rngProvider();
            gr.EnemyMoveRng = rngProvider();
            gr.EnemyBattleRng = rngProvider();
            GetOrCreate(gr).battleLootRng = rngProvider();

        }

        /// <summary>
        /// 
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

            AdvanceNodeRng(gr, jumpToNode.X - cStation.X - 1); // -1 due to node enter advancement?
            AdvanceActRngs(gr, jumpToNode.Act - cStation.Act - (Helpers.IsActTransition(jumpToNode) ? 1 : 0));
        }

        public static void AdvanceNodeRng(GameRunController gr, int steps)
        {
            var nodeRng = GetOrCreate(gr).rootNodeRng;
            for (var i = 0; i < steps; i++)
            {
                AssignNodeRngs(gr, () => new RandomGen(nodeRng.NextULong()));
            }
        }

        public static void AdvanceActRngs(GameRunController gr, int steps)
        {
            var actRng = GetOrCreate(gr).rootActRng;
            for (var i = 0; i < steps; i++)
            {
                AssignActRngs(gr, () => new RandomGen(actRng.NextULong()));
            }
        }

        static public GrRngs GetOrCreate(GameRunController gr)
        {
            var grRgns = table.GetOrCreateValue(gr);
            return grRgns;
        }
    }
}
