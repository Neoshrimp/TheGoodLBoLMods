using HarmonyLib;
using LBoL.ConfigData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.CustomRngs.Sampling
{
    public interface ISlotRequirement<in PT>
    {
        public void PrepReq();

        public bool IsSatisfied(PT type);
    }

    public class ExInPool : ISlotRequirement<Type>
    {
        HashSet<Type> poolSet = new HashSet<Type>();

        public void PrepReq()
        {
            poolSet = new HashSet<Type>(GrRngs.Gr().ExhibitPool);
        }

        public bool IsSatisfied(Type type) => poolSet.Contains(type);
    }

    public class ExHasManaColour : ISlotRequirement<Type>
    {
        public bool IsSatisfied(Type type)
        {
            var exConfig = ExhibitConfig.FromId(type.Name);
            var manaReq = exConfig.BaseManaRequirement;
            return manaReq == null || GrRngs.Gr().BaseMana.HasColor(manaReq.GetValueOrDefault());
        }

        public void PrepReq()
        {
        }
    }

    public class CardInPool : ISlotRequirement<Type>
    {
        public HashSet<Type> poolSet = new HashSet<Type>();
        public bool IsSatisfied(Type type) => poolSet.Contains(type);

        public void PrepReq()
        {
        }
    }


    public class InPool<T> : ISlotRequirement<T>
    {
        public HashSet<T> poolSet = new HashSet<T>();

        public InPool(IEnumerable<T> poolSet)
        {
            this.poolSet = new HashSet<T>(poolSet);
        }

        public bool IsSatisfied(T type) => poolSet.Contains(type);

        public void PrepReq()
        {
        }
    }

    public class InCountedPool<T> : ISlotRequirement<T>
    {
        public Dictionary<T, int> countedPool = new Dictionary<T, int>();

        int count;

        public InCountedPool(IEnumerable<T> types)
        {
            types.Do(t => { 
                this.countedPool.TryAdd(t, 0);
                this.countedPool[t]++;
                count++;
            });
        }

        public int Count { get => count; }

        public bool IsSatisfied(T type)
        {
            return countedPool.ContainsKey(type) && countedPool[type] > 0;
        }

        public void PrepReq()
        {
        }

        public void ReduceCount(T type)
        {
            if (countedPool.ContainsKey(type))
            {
                countedPool[type]--;
                count--;
            }
        }
    }


    public class AdventureInPool : ISlotRequirement<Type>
    {
        public HashSet<Type> poolSet = new HashSet<Type>();
        public bool IsSatisfied(Type type) => poolSet.Contains(type);

        public void PrepReq()
        {
        }
    }

    public class AdventureNOTinHistory : ISlotRequirement<Type>
    {
        public bool IsSatisfied(Type type) => !GrRngs.Gr().AdventureHistory.Contains(type);

        public void PrepReq()
        {
        }
    }
}
