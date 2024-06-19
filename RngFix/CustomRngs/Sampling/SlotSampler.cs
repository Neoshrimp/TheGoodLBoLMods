using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Cards;
using LBoL.Core.GapOptions;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Neutral.White;
using Logging;
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

        List<Type> potentialPool = new List<Type>();


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
            var rollingRng = new RandomGen(rng.NextULong());

            float totalW = 0f;
            float maxW = 0f;

            logInfo = new SamplerLogInfo();

            foreach (var t in potentialPool)
            {
                var w = getW(t);
                if (requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t)))
                {
                    totalW += w;
                    maxW = Math.Max(maxW, w);
                }

                rollingPool.Add(t);
            }

            var wThrehold = rollingRng.NextFloat(0, maxW);


            requirements.Do(r => r.PrepReq());

            //var shitToLog = new List<string>();

            var i = 0;
            while (rollingPool.Count > 0)
            {
                i++;
                var t = rollingPool.Sample(rollingRng);
                var w = getW(t);
/*                if (typeof(T) == typeof(Card))
                {
                    var cc = CardConfig.FromId(t.Name);
                    var gr = GrRngs.Gr();
                    var wt = CardWeightTable.ShopSkillAndFriend.WeightFor(cc, gr.Player.Id, new HashSet<string>());
                    var bw = gr.BaseCardWeight(cc, false);
                    shitToLog.Add($"{t.Name};{String.Join("", cc.Colors)};{w};wt:{wt};bw:{bw}");
                }*/
                

                if (wThrehold < w
                    && requirements.All(r => r.IsSatisfied(t))
                    && (filter == null || filter(t)))
                {
                    logInfo.itemW = w;
                    rezFound = true;
                    rez = initAction(t);
                    break;
                }
            }

            logInfo.maxW = maxW;
            logInfo.wThreshold = wThrehold;
            logInfo.rolls = i;
            logInfo.totalW = totalW;

            if (rezFound)
            {
                if (successAction != null)
                    successAction(rez);

/*                if (rez is YukariFlyObject || rez is SakuyaTea) 
                {
                    shitToLog.Do(s => log.LogDebug(s));
                }*/
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
        public int rolls;
        public float wThreshold;
        public float itemW;
    }


}
