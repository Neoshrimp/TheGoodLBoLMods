using HarmonyLib;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActionRecord;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using Spine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static VariantsC.BepinexPlugin;


namespace VariantsC.Shared
{
    public class ProcessDeckCardsAction : SimpleAction
    {
        Card[] cards;
        Action<IEnumerable<Card>> proccessAction;
        private readonly string actionTitle;
        public int batchSize = 5;
        public float finalDelay = 0.5f;
        public float batchDelay = 0.4f;
        

        int resolutionDepth;

        public ProcessDeckCardsAction(Card[] cards, Action<IEnumerable<Card>> proccessAction, string actionTitle)
        {
            this.cards = cards;
            this.proccessAction = proccessAction;
            this.actionTitle = actionTitle;
        }

        public override void ResolvePhase()
        {
            React(new WaitForCoroutineAction(this.ProcessManyCards()));

            CaputreActionResolverDepth.capDepth = 0;

        }

        public override string ExportDebugDetails()
        {
            StringBuilder pad = new StringBuilder("     ");
            resolutionDepth = CaputreActionResolverDepth.capDepth + 2;
            for (int i = 0; i < resolutionDepth; i++)
                pad.Append("         ");


            return $"{actionTitle}: \n{string.Join("\n", cards.Select(c => string.Concat(new string[] {pad.ToString(), CardColors.ColorName(c)})))}";
        }

        private IEnumerator ProcessManyCards()
        {
            for (int i = 0; i < cards.Length; i += batchSize)
            {
                var upper = Math.Min(cards.Length, i + batchSize);
                var display = cards[i..upper];
                proccessAction(display);
                if (upper < cards.Length)
                {
                    yield return new WaitForSecondsRealtime(batchDelay + 0.2f * batchSize);
                }
            }
            yield return new WaitForSecondsRealtime(finalDelay);

        }



        [HarmonyPatch(typeof(ActionResolver), nameof(ActionResolver.InternalResolve))]
        class CaputreActionResolverDepth
        {
            public static int capDepth = 0;
            static void Prefix(BattleAction action, int depth)
            {
                if (action.GetType() == typeof(ProcessDeckCardsAction))
                {
                    capDepth = depth;
                }

            }
        }


    }

    
}
