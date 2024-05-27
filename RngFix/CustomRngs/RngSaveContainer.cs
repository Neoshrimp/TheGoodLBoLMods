using LBoL.Base;
using LBoL.Core;
using LBoLEntitySideloader.PersistentValues;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.CustomRngs
{
    public class RngSaveContainer : CustomGameRunSaveData
    {
        public override void Restore(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            grRngs.rootNodeRng = RandomGen.FromState(rootNodeRngState);
            grRngs.rootActRng = RandomGen.FromState(rootActRngState);

            grRngs.enemyActRng = RandomGen.FromState(enemyActRngState);
            grRngs.eliteActRng = RandomGen.FromState(eliteActRngState);
            grRngs.eventActRng = RandomGen.FromState(eventActRngState);

        }

        public override void Save(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            rootNodeRngState = grRngs.rootNodeRng.State;
            rootActRngState = grRngs.rootActRng.State;

            enemyActRngState = grRngs.enemyActRng.State;
            eliteActRngState = grRngs.eliteActRng.State;
            eventActRngState = grRngs.eventActRng.State;
        }

        public ulong rootNodeRngState;
        public ulong rootActRngState;

        public ulong enemyActRngState;
        public ulong eliteActRngState;
        public ulong eventActRngState;
    }
}
