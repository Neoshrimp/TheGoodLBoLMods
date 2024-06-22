using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Neutral.White;
using Logging;
using RngFix.Patches.Debug;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static RngFix.BepinexPlugin;

namespace RngFix.CustomRngs.Sampling
{
    public class SlotSampler<T>
    {
        public List<ISlotRequirement> requirements = new List<ISlotRequirement>();
        Func<Type, T> initAction;
        public Action<T> successAction = null;
        public Action failureAction = null;

        public Action<int, Type, float> debugAction = null;

        List<Type> potentialPool = new List<Type>();

        public uint maxWRollAttemts = (uint)1E7;
        public bool fullRoll = false;


        public SlotSampler(List<ISlotRequirement> requirements, Func<Type, T> initAction, Action<T> successAction, Action failureAction, List<Type> potentialPool)
        {
            this.requirements = requirements;
            this.initAction = initAction;
            this.successAction = successAction;
            this.failureAction = failureAction;
            this.potentialPool = potentialPool;

        }



        public T Roll(RandomGen rng, Func<Type, float> getW, out SamplerLogInfo logInfo, Predicate<Type> filter = null, Func<T> fallback = null)
        {
            T rez = default;
            bool rezFound = false;
            var rollingPool = new UniformUniqueRandomPool<Type>();


            var itemRollingRng = new RandomGen(rng.NextULong());
            var wRollingRng = new RandomGen(itemRollingRng.NextULong());

            float rawTotalW = 0f;
            float rawMaxW = 0f;

            float possibleTotalW = 0f;
            float possibleMaxW = 0f;

            logInfo = new SamplerLogInfo();

            requirements.Do(r => r.PrepReq());

            foreach (var t in potentialPool)
            {
                var w = getW(t);
                if (requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t)))
                {

                    possibleTotalW += w;
                    possibleMaxW = Math.Max(possibleMaxW, w);
                }
                rawTotalW += w;
                rawMaxW = Math.Max(rawMaxW, w);

                rollingPool.Add(t);
            }


            logInfo.rawMaxW = rawMaxW;
            logInfo.totalW = possibleTotalW;
            logInfo.maxW = possibleMaxW;


            var wThrehold = 0f;
            uint wAttemts = 0;

            // idk if this does anything
            bool wFound = false;
            while (wAttemts < maxWRollAttemts)
            {

                var w = itemRollingRng.NextFloat(0, rawMaxW);
                if (w <= possibleMaxW)
                {
                    wThrehold = w;
                    wFound = true;
                    break;
                }

                wAttemts++;
            }

            if (!wFound)
            {
                log.LogWarning($"Possible weight not found in {maxWRollAttemts}. Using a safe roll.");
                wThrehold = itemRollingRng.NextFloat(0, possibleMaxW);
            }

            logInfo.wRollAttempts = wAttemts;
            logInfo.wThreshold = wThrehold;


            var i = 0;
            while (rollingPool.Count > 0)
            {
                i++;
                var t = rollingPool.Sample(itemRollingRng);
                var w = getW(t);

                debugAction?.Invoke(i, t, w);


                if (wThrehold < w
                    && requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t))
                    && !rezFound)
                {
                    logInfo.itemW = w;
                    rezFound = true;
                    rez = initAction(t);
                    if(!fullRoll)
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


    public class SamplerLogInfo
    {
        public float totalW;
        public float maxW;
        public float rawMaxW;
        public int rolls;
        public float wThreshold;
        public float itemW;

        public uint wRollAttempts;

        public override string ToString()
        {
            return $"ItemW:{itemW};WThreshold:{wThreshold};MaxW:{maxW};rawMaxW:{rawMaxW};TotalW:{totalW};wRollAttempts:{wRollAttempts};";
        }
    }


}
