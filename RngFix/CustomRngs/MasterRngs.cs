using LBoL.Base;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static RngFix.BepinexPlugin;



namespace RngFix.CustomRngs
{

    public class StageMasterRng : MasterRng
    {
        public StageMasterRng() : base() { }
        public StageMasterRng(ulong seed) : base(seed) { }

        public override void Advance(GameRunController gr)
        {
            var grRngs = GrRngs.GetOrCreate(gr);

            //log.LogDebug($"advancing stage rng. {rng.State}");

            grRngs.persRngs.eliteQueueRng = new RandomGen(rng.NextULong());
            grRngs.persRngs.adventureQueueSeed = rng.NextULong();

            grRngs.persRngs.eliteInitRng = new RandomGen(rng.NextULong());
            grRngs.persRngs.bossInitRng = new RandomGen(rng.NextULong());

            // for testing consistency
            rng.NextULong();
            //grRngs.persRngs.adventureInitRng = new RandomGen(rng.NextULong());
            grRngs.persRngs.transitionInitRng = new RandomGen(rng.NextULong());
        }
    }

    public class ActMasterRng : MasterRng
    {
        public ActMasterRng() : base() { }
        public ActMasterRng(ulong seed) : base(seed) { }

        public override void Advance(GameRunController gr)
        {
            var grRngs = GrRngs.GetOrCreate(gr);

            //log.LogDebug($"advancing act rng. {rng.State}");

            grRngs.persRngs.enemyActQueueRng = new RandomGen(rng.NextULong());
            grRngs.persRngs.battleInitRng = new RandomGen(rng.NextULong());
        }
    }

    public class NodeMasterRng : MasterRng
    {
        public NodeMasterRng() : base() { }
        public NodeMasterRng(ulong seed) : base(seed) { }

        public override void Advance(GameRunController gr)
        {
            var grRngs = GrRngs.GetOrCreate(gr);

            //log.LogDebug($"node master state BEFORE: {rng.State}");

            grRngs.persRngs.prevNodeMasterState = rng.State;

            gr.GameRunEventRng = new RandomGen(rng.NextULong()); // almost unused after all the patches
            gr.BattleRng = new RandomGen(rng.NextULong());
            gr.BattleCardRng = new RandomGen(rng.NextULong());
            gr.ShuffleRng = new RandomGen(rng.NextULong());
            gr.EnemyMoveRng = new RandomGen(rng.NextULong());
            gr.EnemyBattleRng = new RandomGen(rng.NextULong());
            grRngs.battleLootRng = new RandomGen(rng.NextULong());

            gr.AdventureRng = new RandomGen(rng.NextULong());
            grRngs.transitionRng = new RandomGen(rng.NextULong());

            grRngs.bossCardRewardRng = new RandomGen(rng.NextULong()); // unused
            grRngs.moneyRewardRng = new RandomGen(rng.NextULong());


            grRngs.unusedNode0 = new RandomGen(rng.NextULong());
            grRngs.unusedNode1 = new RandomGen(rng.NextULong());
            grRngs.unusedNode2 = new RandomGen(rng.NextULong());
            grRngs.unusedNode3 = new RandomGen(rng.NextULong());
            grRngs.unusedNode4 = new RandomGen(rng.NextULong());

            //log.LogDebug($"node master state AFTER: {rng.State}");


        }
    }
}
