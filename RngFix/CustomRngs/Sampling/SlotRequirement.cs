using LBoL.ConfigData;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.CustomRngs.Sampling
{
    public interface ISlotRequirement
    {
        public void PrepReq();

        public bool IsSatisfied(Type type);
    }

    public class ExInPool : ISlotRequirement
    {
        HashSet<Type> poolSet = new HashSet<Type>();

        public void PrepReq()
        {
            poolSet = new HashSet<Type>(GrRngs.Gr().ExhibitPool);
        }

        public bool IsSatisfied(Type type) => poolSet.Contains(type);
    }

    public class ExHasManaColour : ISlotRequirement
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

    public class CardInPool : ISlotRequirement
    {
        public HashSet<Type> poolSet = new HashSet<Type>();
        public bool IsSatisfied(Type type) => poolSet.Contains(type);

        public void PrepReq()
        {
        }
    }


    public class AdventureInPool : ISlotRequirement
    {
        public HashSet<Type> poolSet = new HashSet<Type>();
        public bool IsSatisfied(Type type) => poolSet.Contains(type);

        public void PrepReq()
        {
        }
    }

    public class AdventureNOTinHistory : ISlotRequirement
    {
        public bool IsSatisfied(Type type) => !GrRngs.Gr().AdventureHistory.Contains(type);

        public void PrepReq()
        {
        }
    }
}
