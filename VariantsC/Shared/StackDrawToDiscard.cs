using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using HarmonyLib;
using LBoL.Presentation.UI.Widgets;
using LBoL.Presentation;
using LBoLEntitySideloader.UIAdditions.CardUIAdds;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using LBoL.Core;

namespace VariantsC.Shared
{
    public class StackDrawToDiscard : EventBattleAction<GameEventArgs>
    {
        public override bool IsCanceled => false;

        public StackDrawToDiscard()
        {
            Args = new GameEventArgs() { };
        }

        public void ResolvePhase()
        {
            foreach (Card card in Battle._drawZone)
            {
                card.Zone = CardZone.Discard;
            }
            Battle._discardZone.AddRange(Battle._drawZone);
            Battle._drawZone.Clear();

            
        }

        public override IEnumerable<Phase> GetPhases()
        {
            yield return CreatePhase("Main", ResolvePhase, hasViewer: false);
            yield return CreatePhase("Visual", () => { }, hasViewer: true);

        }

        static class Viewer
        {
            static public IEnumerator ViewMove(StackDrawToDiscard action, CardUi cardUi)
            {
                if (cardUi.DrawCount > 0)
                {
                    AudioManager.PlayUi("CardReshuffle", false);
                    int count = Math.Min(cardUi.DrawCount, 10);
                    for (int i = 0; i < count; i++)
                    {
                        AudioManager.PlayUi("CardFly", false);
                        Transform parent = UnityEngine.Object.Instantiate<Transform>(cardUi.cardFlyHelperPrefab, cardUi.cardEffectLayer);
                        parent.localPosition = cardUi.discardButton.transform.localPosition;
                        CardFlyBrief clone = Object.Instantiate<CardFlyBrief>(cardUi.cardFlyBrief, parent);
                        Transform transform = clone.transform;

                        parent.DOLocalMove(cardUi.discardButton.transform.localPosition, 0.5f, false).SetEase(Ease.InSine).OnComplete(delegate
                        {
                            clone.CloseCard();
                            Object.Destroy(parent.gameObject, 0.5f);
                        });
                        int num = UnityEngine.Random.Range(100, 300);
                        transform.DOLocalMoveY((float)num, 0.25f, false).SetEase(Ease.OutSine).SetRelative(true)
                            .SetLoops(2, LoopType.Yoyo);
                        yield return new WaitForSecondsRealtime(0.05f);
                    }
                    yield return new WaitForSecondsRealtime(0.3f);
                    cardUi.DrawCount = cardUi.Battle.DrawZone.Count;
                    cardUi.DiscardCount = cardUi.Battle.DiscardZone.Count;
                    cardUi.ExileCount = cardUi.Battle.ExileZone.Count;
                }
                cardUi.RefreshAll();
                yield break;
            }
        }

        [HarmonyPatch(typeof(CardUi), nameof(CardUi.EnterBattle))]
        class CardUiEnterBattle_Patch
        {

            static void Postfix(CardUi __instance)
            {
                var d = CardUiExtensions.BAV_DelegateWrap<StackDrawToDiscard>(Viewer.ViewMove, __instance);
                __instance.Battle.ActionViewer.Register(d);
            }
        }

        [HarmonyPatch(typeof(CardUi), nameof(CardUi.LeaveBattle))]
        class CardUiLeaveBattle_Patch
        {

            static void Prefix(CardUi __instance)
            {
                var d = CardUiExtensions.BAV_DelegateWrap<StackDrawToDiscard>(Viewer.ViewMove, __instance);
                __instance.Battle.ActionViewer.Unregister(d);
            }

        }
    }
}
