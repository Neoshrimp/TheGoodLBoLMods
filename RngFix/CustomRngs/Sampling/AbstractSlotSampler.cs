using LBoL.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace RngFix.CustomRngs.Sampling
{
    public abstract class AbstractSlotSampler<T, PT>
    {

        public List<ISlotRequirement<PT>> requirements = new List<ISlotRequirement<PT>>();
        public Func<PT, T> initAction;
        public Action<T> successAction = null;
        public Action failureAction = null;

        public Action<SamplerLogInfo, PT> debugAction = null;

        

        



        public AbstractSlotSampler(List<ISlotRequirement<PT>> requirements, Func<PT, T> initAction, Action<T> successAction, Action failureAction, IEnumerable<PT> potentialPool)
        {
            this.requirements = requirements;
            this.initAction = initAction;
            this.successAction = successAction;
            this.failureAction = failureAction;
            BuildPool(potentialPool);
        }

        public abstract T Roll(RandomGen rng, Func<PT, float> getW, out SamplerLogInfo logInfo, Predicate<PT> filter = null, Func<T> fallback = null, float presetMaxW = 0f);


        public abstract void BuildPool(IEnumerable<PT> potentialPool);

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
            return $"ItemW:{itemW};WThreshold:{wThreshold};MaxW:{maxW};itemProb:{itemProb};passThreshold:{passThreshold};probabilityFraction:{probabilityFraction};probabiltyMul:{probabiltyMul};rawMaxW:{rawMaxW};TotalW:{totalW};wRollAttempts:{wRollAttempts};rolls:{rolls};fractionWarning:{fractionWarning}";
        }
    }
}
