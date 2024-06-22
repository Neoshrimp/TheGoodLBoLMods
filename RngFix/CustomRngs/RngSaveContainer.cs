using LBoL.Base;
using LBoL.Core;
using LBoLEntitySideloader.PersistentValues;
using LBoLEntitySideloader.PersistentValues.TypeConverters;
using RngFix.Patches.Debug;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace RngFix.CustomRngs
{
    public class RngSaveContainer : CustomGameRunSaveData
    {
        public override void Restore(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            grRngs.persRngs = persRngs;

            grRngs.persRngs.exhibitSelfRngs.GetId = ex => ex.Id;
            grRngs.persRngs.adventureSelfRngs.GetId = adv => adv.Id;

            StatsLogger.currentGrId = currentGrId;
        }

        public override void Save(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            persRngs = grRngs.persRngs;
            currentGrId = StatsLogger.currentGrId;
        }

        public override IEnumerable<IYamlTypeConverter> TypeConverters()
        {
            yield return new RandomGenTypeConverter();
        }


        public GrRngs.PersRngs persRngs;

        public string currentGrId;

    }
}
