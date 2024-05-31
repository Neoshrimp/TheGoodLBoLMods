using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RngFix.CustomRngs
{
    public class ExhibitSelfRngs
    {
        public RandomGen rootRng;

        public ExhibitSelfRngs() { }
        public ExhibitSelfRngs(ulong rootSeed) : this()
        {
            this.rootRng = new RandomGen(rootSeed);
        }

        public Dictionary<string, RandomGen> exRngs = new Dictionary<string, RandomGen>();


        public void InitialiseExRngs()
        {
            ExhibitConfig.AllConfig().OrderBy(ec => ec.Order).Do(ec => {
                exRngs.TryAdd(ec.Id, new RandomGen(rootRng.NextULong()));
            });
        }

        public RandomGen GetRng(Exhibit exhibit) => GetRng(exhibit.Id);

        public RandomGen GetRng(string id)
        {
            if (!exRngs.TryGetValue(id, out RandomGen gen))
            {
                gen = new RandomGen(rootRng.NextULong());
                exRngs[id] = gen;
            }
            return gen;
        }

        internal static RandomGen GetSelfRng(GameRunController gr, Exhibit ex) => GrRngs.GetOrCreate(gr).ExhibitSelfRngs.GetRng(ex);

        internal static RandomGen GetSelfRng(GameRunController gr, string id) => GrRngs.GetOrCreate(gr).ExhibitSelfRngs.GetRng(id);



    }
}
