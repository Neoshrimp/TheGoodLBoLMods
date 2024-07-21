using HarmonyLib;
using LBoL.Base;
using LBoL.EntityLib.Exhibits.Common;
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.CustomRngs.Sampling.UniformPools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.CustomRngs.Sampling
{
    public class WeightedSlotSampler<T> : AbstractSlotSampler<T, Type> where T : class
    {
        RepeatableUniformRandomPool<Type> constantPotentialPool;

        public ProbFactionRange factionRange = new ProbFactionRange(new float[] { 1.6f, 2, 4, 5, 6, 7, 10, 20, 100 });

        public uint maxItemRolls = (uint)1E6;

        // 0 - possibleMaxW, 1 - possibleTotalW, unused
        public float probabiltyMul = 1-0.618033988749f;

        public uint extraRolls = 0;


        public WeightedSlotSampler(List<ISlotRequirement<Type>> requirements, Func<Type, T> initAction, Action<T> successAction, Action failureAction, IEnumerable<Type> potentialPool) : base(requirements, initAction, successAction, failureAction, potentialPool)
        {
        }

        public override void BuildPool(IEnumerable<Type> potentialPool)
        {
            this.constantPotentialPool = new RepeatableUniformRandomPool<Type>();
            this.constantPotentialPool.AddRange(potentialPool);
        }

        override public T Roll(RandomGen rng, Func<Type, float> getW, out SamplerLogInfo logInfo, Predicate<Type> filter = null, Func<T> fallback = null)
        {
            T rez = default;
            bool rezFound = false;


            var itemRollingRng = new RandomGen(rng.NextULong());
            var wRollingRng = new RandomGen(itemRollingRng.NextULong());

            float rawTotalW = 0f;
            float rawMaxW = 0f;

            float possibleTotalW = 0f;
            float possibleMaxW = 0f;

            logInfo = new SamplerLogInfo();

            requirements.Do(r => r.PrepReq());

            Func<Type, float> getWwrap = t => t == null ? 0f : getW(t);

            foreach (var re in constantPotentialPool)
            {
                var t = re.Elem;
                var w = getWwrap(t);
                if (w > 0
                    && requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t)))
                {

                    possibleTotalW += w;
                    possibleMaxW = Math.Max(possibleMaxW, w);
                }
                rawTotalW += w;
                rawMaxW = Math.Max(rawMaxW, w);
            }


            logInfo.rawMaxW = rawMaxW;
            logInfo.totalW = possibleTotalW;
            logInfo.maxW = possibleMaxW;

            //var probabilityFraction = possibleMaxW + Math.Max(0, possibleTotalW - possibleMaxW) * probabiltyMul;

            var rangeRez = factionRange.GetFraction(possibleMaxW);
            float probabilityFraction = rangeRez.Item1;

            logInfo.fractionWarning = rangeRez.Item2;
            if (rangeRez.Item2 != "")
                BepinexPlugin.log.LogWarning(rangeRez.Item2);

            uint i = 0;
            uint er = 0;
            while (i < maxItemRolls)
            {
                i++;
                var t = constantPotentialPool.Sample(itemRollingRng);
                var w = getWwrap(t);
                var itemProb = w / probabilityFraction;
                var passThreshold = wRollingRng.NextFloat(0, 1);

/*                if (debugAction != null)
                {
                    var rollLI = new SamplerLogInfo()
                    {
                        totalW = possibleMaxW,
                        maxW = possibleMaxW,
                        rawMaxW = rawMaxW,
                        rolls = i,
                        wThreshold = 0,
                        itemW = w,

                        wRollAttempts = 0,

                        itemProb = itemProb,
                        passThreshold = passThreshold,
                        probabilityFraction = probabilityFraction,
                        probabiltyMul = probabiltyMul,
                    };

                    debugAction.Invoke(rollLI, t);
                }*/

                if (passThreshold < itemProb
                    && requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t))
                    && !rezFound)
                {
                    logInfo.itemW = w;
                    logInfo.itemProb = itemProb;
                    logInfo.passThreshold = passThreshold;
                    logInfo.probabilityFraction = probabilityFraction;
                    logInfo.probabiltyMul = probabiltyMul;

                    rezFound = true;
                    rez = initAction(t);
                    if(extraRolls == 0)
                        break;
                }

                if (rezFound)
                { 
                    if(er < extraRolls)
                        er++;
                    else
                        break;
                }
            }



            logInfo.rolls = i;


            if (rezFound)
            {
                if (successAction != null)
                    successAction(rez);

            }
            else
            {
                if (fallback != null)
                    rez = fallback();
                if (failureAction != null)
                    failureAction();
            }


            return rez;
        }
    }
}
