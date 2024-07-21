using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Exhibits.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace RngFix.CustomRngs
{
    public class EntitySelfRngs<T>
    {
        public EntitySelfRngs()
        {
        }

        public EntitySelfRngs(Func<T, string> GetId) : this()
        { 
            this.GetId = GetId;
        }

        public EntitySelfRngs(Func<T, string> GetId, ulong rootSeed) : this(GetId)
        {
            this.rootRng = new RandomGen(rootSeed);
        }

        public RandomGen rootRng;
        [YamlIgnore]
        public Func<T, string> GetId;

        public Dictionary<string, RandomGen> rngs = new Dictionary<string, RandomGen>();


        public void InitialiseRngs(IEnumerable<string> initOrder)
        {
            initOrder.Do(id => {
                if (id == null)
                { 
                    rootRng.NextULong();
                    return;
                }
                rngs.TryAdd(id, new RandomGen(rootRng.NextULong()));
            });
        }

        public RandomGen GetRng(T entity) => GetRng(GetId(entity));

        public RandomGen GetRng(string id)
        {
            if (!rngs.TryGetValue(id, out RandomGen gen))
            {
                throw new ArgumentException($"Rng with id {id} not found.");
            }
            return gen;
        }

      

    }

    internal static class ExhibitsSelfRngs
    {
        internal static string GetId(Exhibit ex) => ex.Id;

        internal static RandomGen GetSelfRng(GameRunController gr, Exhibit entity) => GrRngs.GetOrCreate(gr).ExhibitSelfRngs.GetRng(entity);

        internal static RandomGen GetSelfRng(GameRunController gr, string id) => GrRngs.GetOrCreate(gr).ExhibitSelfRngs.GetRng(id);
    }

    internal static class AdventureSelfRngs
    {
        internal static string GetId(Adventure adv) => adv.Id;
    }
}
