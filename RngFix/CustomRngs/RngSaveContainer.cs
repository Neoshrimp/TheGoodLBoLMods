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

        }

        public override void Save(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            rootBattleRngState = grRngs.rootBattleRng.State;
            rootStationRngState = grRngs.rootStationRng.State;
        }

        public ulong rootBattleRngState;
        public ulong rootStationRngState;
    }
}
