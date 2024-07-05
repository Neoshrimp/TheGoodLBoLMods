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
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.CustomRngs.Sampling.UniformPools;
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
    public class UniformSlotSampler<T, PT> : AbstractSlotSampler<T, PT> where T : class where PT : class
    {

        List<PT> potentialPool;

        public bool fullRoll = false;


        public UniformSlotSampler(List<ISlotRequirement<PT>> requirements, Func<PT, T> initAction, Action<T> successAction, Action failureAction, IEnumerable<PT> potentialPool) : base(requirements, initAction, successAction, failureAction, potentialPool)
        {
        }

        public override void BuildPool(IEnumerable<PT> potentialPool)
        {
            this.potentialPool = new List<PT>(potentialPool);

        }


        override public T Roll(RandomGen rng, Func<PT, float> getW, out SamplerLogInfo logInfo, Predicate<PT> filter = null, Func<T> fallback = null)
        {
            T rez = default;
            bool rezFound = false;
            var rollingPool = new UniformUniqueRandomPool<PT>();

            getW = t => 1f;

            var itemRollingRng = new RandomGen(rng.NextULong());


            logInfo = new SamplerLogInfo();

            requirements.Do(r => r.PrepReq());

            foreach (var t in potentialPool)
            {
                rollingPool.Add(t);
            }


            uint i = 0;
            while (rollingPool.Count > 0)
            {
                i++;
                var t = rollingPool.Sample(itemRollingRng);

                if (t != null
                    && requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t))
                    && !rezFound)
                {
                    rezFound = true;
                    rez = initAction(t);
                    if (!fullRoll)
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
