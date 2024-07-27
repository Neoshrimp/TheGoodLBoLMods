using LBoL.Base;
using LBoL.Core;
using LBoLEntitySideloader.PersistentValues;
using LBoLEntitySideloader.PersistentValues.TypeConverters;
using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace VariantsC.Rng
{
    public class SaveContainer : CustomGameRunSaveData
    {
        public override void Restore(GameRunController gameRun)
        {
            PerRngs.Assign(gameRun, persistentRngs);
        }

        public override void Save(GameRunController gameRun)
        {
            persistentRngs = PerRngs.Get(gameRun);
        }

        public override IEnumerable<IYamlTypeConverter> TypeConverters()
        {
            yield return new RandomGenTypeConverter();
        }


        public PerRngs persistentRngs;


    }
}
