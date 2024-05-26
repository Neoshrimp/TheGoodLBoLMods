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
            grRngs.rootBattleRng = RandomGen.FromState(rootBattleRngState);
            grRngs.rootStationRng = RandomGen.FromState(rootStationRngState);

            grRngs.enemyStationRng = RandomGen.FromState(enemyStationRngState);
            grRngs.eliteStationRng = RandomGen.FromState(eliteStationRngState);
            grRngs.eventStationRng = RandomGen.FromState(eventStationRngState);

        }

        public override void Save(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            rootBattleRngState = grRngs.rootBattleRng.State;
            rootStationRngState = grRngs.rootStationRng.State;

            enemyStationRngState = grRngs.enemyStationRng.State;
            eliteStationRngState = grRngs.eliteStationRng.State;
            eventStationRngState = grRngs.eventStationRng.State;
        }

        public ulong rootBattleRngState;
        public ulong rootStationRngState;

        public ulong enemyStationRngState;
        public ulong eliteStationRngState;
        public ulong eventStationRngState;
    }
}
