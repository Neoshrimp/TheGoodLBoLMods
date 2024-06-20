using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.CustomRngs.Sampling
{
    public static class SamplerDebug
    {
        public static void SimulateCardRoll(ulong state, CardWeightTable weightTable)
        {
            var gr = GrRngs.Gr();

            if (gr == null)
                return;

            var grRgns = GrRngs.GetOrCreate(gr);
            var sampler = grRgns.CardSampler;
            var rng = RandomGen.FromState(state);

            var cardPoolReq = sampler.requirements.Find(r => r is CardInPool) as CardInPool;

            cardPoolReq.poolSet = new HashSet<Type>(gr.CreateValidCardsPool(weightTable, new ManaGroup?(gr.BaseMana), gr.RewardAndShopCardColorLimitFlag == 0, false, false, null).Select(re => re.Elem));

            var charExSet = new HashSet<string>(gr.Player.Exhibits.Where(e => e.OwnerId != null).Select(e => e.OwnerId));

            bool logWBreakdown = false;

            Func<Type, float> getW = t => {
                var cc = CardConfig.FromId(t.Name);
                var wt = weightTable.WeightFor(cc, gr.Player.Id, charExSet);
                var bw = gr.BaseCardWeight(cc, false);
                if(logWBreakdown)
                    log.LogDebug($"weightT:{wt};baseW:{bw}");
                return wt * bw;
            };

            sampler.debugAction = (i, t, w) => {
                logWBreakdown = true;
                log.LogDebug($"i:{i};card:{t};colors:{string.Join("", CardConfig.FromId(t.Name).Colors)};w:{w};"); 
            };

            sampler.Roll(rng, getW, out var logInfo);


            log.LogDebug(logInfo);

            sampler.debugAction = null;

        }
    }
}
