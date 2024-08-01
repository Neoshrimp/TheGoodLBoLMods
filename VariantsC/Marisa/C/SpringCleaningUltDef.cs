using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using VariantsC.Rng;
using VariantsC.Sakuya.C;
using VariantsC.Shared;
using System.Linq;
using HarmonyLib;
using LBoL.Base.Extensions;
using System.Collections;
using static VariantsC.BepinexPlugin;
using LBoL.Core.Battle.BattleActionRecord;

namespace VariantsC.Marisa.C
{
    public sealed class SpringCleaningUltDef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(SpringCleaningUlt);

        public override LocalizationOption LoadLocalization() => BepinexPlugin.UltimateSkillBatchLoc.AddEntity(this);

        public override Sprite LoadSprite() => ResourceLoader.LoadSprite("SpringCleaningUlt.png", BepinexPlugin.embeddedSource);

        public override UltimateSkillConfig MakeConfig()
        {
            return new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 200,
                PowerPerLevel: 200,
                MaxPowerLevel: 1,
                RepeatableType: UsRepeatableType.OncePerTurn,
                Damage: 0,
                Value1: 7,
                Value2: 2,
                Keywords: Keyword.Battlefield,
                RelativeEffects: new List<string>() {  },
                RelativeCards: new List<string>() { }
            );
        }
    }

    [EntityLogic(typeof(SpringCleaningUltDef))]
    public sealed class SpringCleaningUlt : UltimateSkill
    {
        public SpringCleaningUlt()
        {
            TargetType = TargetType.Self;
        }

        ManaGroup _mana = new ManaGroup { Philosophy = 3 };
        public ManaGroup Mana { get => _mana; }

        public int HalfDeck { get => GameRun.BaseDeck.Count / 2; }
        public string HalfDeckWrap { get => GameRun == null ? "" : $"(<color=#B2FFFF>{HalfDeck}</color>)"; }

        public int ToRemove { get => HalfDeck / 2; }
        public string ToRemoveWrap { get => GameRun == null ? "" : $"(<color=#B2FFFF>{ToRemove}</color>)"; }

        // rounds up
        public int ToUpgrade { get => ((HalfDeck-ToRemove)-1)/2 + 1; }
        public string ToUpgradeWrap { get => GameRun == null ? "" : $"(<color=#B2FFFF>{ToUpgrade}</color>)"; }


        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {
            yield return PerformAction.Spell(Owner, Id);
            yield return PerformAction.Effect(Owner, "MoonG", sfxId: "MarisaBottleLaunch");


            
            var subRootRng = new RandomGen(PerRngs.Get(GameRun).springCleaningRng.NextULong());

            var selected = GameRun.BaseDeckWithOutUnremovable
                .Where(c => c.Id != nameof(EverHoardingCard))
                .GroupBy(c => c.IsUpgraded.ToInt() * 1 /*+ c.IsBasic.ToInt() * 2*/)
                .OrderBy(g => g.Key)
                .Select(g => { var l = g.ToList(); l.Shuffle(new RandomGen(subRootRng.NextULong())); return l; })
                .SelectMany(l => l)
                .Take(HalfDeck)
                .ToList();

            var cardsToRemove = selected.GetRange(0, ToRemove).ToArray();
            var cardsToRemoveIdSet = cardsToRemove.Select(c => c.InstanceId).ToHashSet();

            var cardsToUpgrade = selected.GetRange(ToRemove, selected.Count - ToRemove).Where(c => c.CanUpgradeAndPositive).Take(ToUpgrade).ToArray();
            var cardsToUpgradeIdSet = cardsToUpgrade.Select(c => c.InstanceId).ToHashSet();

            yield return new ProcessDeckCardsAction(cardsToRemove, cards => GameRun.RemoveDeckCards(cards, true), "Removed");
            yield return new ProcessDeckCardsAction(cardsToUpgrade, cards => GameRun.UpgradeDeckCards(cards, true), "Upgraded");


/*            yield return new ProcessDeckCardsAction(ProcessManyCards(cardsToRemove, cards => GameRun.RemoveDeckCards(cards, true), 0.5f));
            yield return new ProcessDeckCardsAction(ProcessManyCards(cardsToUpgrade, cards => GameRun.UpgradeDeckCards(cards, true), 0.5f));*/



            foreach (var c in Battle.EnumerateAllCards().ToList())
            {
                if (cardsToRemoveIdSet.Contains(c.InstanceId))
                {
                    yield return new RemoveCardAction(c);
                }
                else if (cardsToUpgradeIdSet.Contains(c.InstanceId) && c.CanUpgrade)
                {
                    //yield return new UpgradeCardAction(c);
                    c.Upgrade(); // faster ui
                }
            }


            var misfortune = GameRun.GetRandomCurseCard(subRootRng, true);
            GameRun.AddDeckCard(misfortune, true);

            yield return new DiscardManyAction(Battle.HandZone);
            yield return new StackDrawToDiscard();

            yield return new DrawManyCardAction(base.Value1);
            yield return new GainManaAction(Mana);

        }



        private IEnumerator ProcessManyCards(Card[] cards, Action<IEnumerable<Card>> proccessAction, float finalDelay)
        {
            int batchSize = 5;
            for (int i = 0; i < cards.Length; i += batchSize)
            {
                var upper = Math.Min(cards.Length, i + batchSize);
                var display = cards[i..upper];
                proccessAction(display);
                if (upper < cards.Length)
                {
                    yield return new WaitForSecondsRealtime(0.5f + 0.2f * batchSize);
                }
            }
            yield return new WaitForSecondsRealtime(finalDelay);

        }
    }
}
