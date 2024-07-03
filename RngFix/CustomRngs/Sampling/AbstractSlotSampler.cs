using LBoL.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.CustomRngs.Sampling
{
    public abstract class AbstractSlotSampler<T>
    {

        public List<ISlotRequirement> requirements = new List<ISlotRequirement>();
        protected Func<Type, T> initAction;
        public Action<T> successAction = null;
        public Action failureAction = null;

        public Action<SamplerLogInfo, Type> debugAction = null;

        

        



        public AbstractSlotSampler(List<ISlotRequirement> requirements, Func<Type, T> initAction, Action<T> successAction, Action failureAction)
        {
            this.requirements = requirements;
            this.initAction = initAction;
            this.successAction = successAction;
            this.failureAction = failureAction;
        }

        public abstract T Roll(RandomGen rng, Func<Type, float> getW, out SamplerLogInfo logInfo, Predicate<Type> filter = null, Func<T> fallback = null);

    }

    public class SamplerLogInfo
    {
        public float totalW;
        public float maxW;
        public float rawMaxW;
        public uint rolls;
        public float wThreshold;
        public float itemW;

        public uint wRollAttempts;

        public float itemProb;
        public float passThreshold;
        public float probabilityFraction;
        public float probabiltyMul;
        public string fractionWarning;

        public override string ToString()
        {
            return $"ItemW:{itemW};WThreshold:{wThreshold};MaxW:{maxW};itemProb:{itemProb};passThreshold:{passThreshold};probabilityFraction:{probabilityFraction};probabiltyMul:{probabiltyMul};rawMaxW:{rawMaxW};TotalW:{totalW};wRollAttempts:{wRollAttempts};fractionWarning:{fractionWarning}";
        }
    }
}
