using LBoL.Base;
using LBoL.Core;
using LBoLEntitySideloader.PersistentValues;
using LBoLEntitySideloader.PersistentValues.TypeConverters;
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
        }

        public override void Save(GameRunController gameRun)
        {
            var grRngs = GrRngs.GetOrCreate(gameRun);
            persRngs = grRngs.persRngs;

        }

        public override IEnumerable<IYamlTypeConverter> TypeConverters()
        {
            yield return new RandomGenTypeConverter();
        }


        public GrRngs.PersRngs persRngs;


    }
}
