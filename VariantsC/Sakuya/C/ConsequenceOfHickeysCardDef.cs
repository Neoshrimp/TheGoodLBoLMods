using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using static VariantsC.BepinexPlugin;

namespace VariantsC.Sakuya.C
{
    public sealed class ConsequenceOfHickeysCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(ConsequenceOfHickeysCard);

        public override CardImages LoadCardImages() => new CardImages(embeddedSource, ResourceLoader.LoadTexture("ConsequencesOfHickeysCard.png", embeddedSource));

        public override LocalizationOption LoadLocalization() => new GlobalLocalization(embeddedSource);


        public override CardConfig MakeConfig() 
        {
            return new CardConfig(
                Index: 0,
                Id: "",
                Order: 10,
                AutoPerform: true,
                Perform: new string[0][],
                GunName: "Simple1",
                GunNameBurst: "Simple2",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: poolNewCards.Value,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Common,
                Type: CardType.Attack,
                TargetType: TargetType.SingleEnemy,
                Colors: new List<ManaColor>() { ManaColor.Red },
                IsXCost: false,
                Cost: new ManaGroup() { Red = 1 },
                UpgradedCost: null,
                MoneyCost: null,
                Damage: 3,
                UpgradedDamage: null,
                Block: null,
                UpgradedBlock: null,
                Shield: null,
                UpgradedShield: null,
                Value1: 2,
                UpgradedValue1: 1,
                Value2: 2,
                UpgradedValue2: 3,
                Mana: null,
                UpgradedMana: null,
                Scry: null,
                UpgradedScry: null,
                ToolPlayableTimes: null,
                Loyalty: null,
                UpgradedLoyalty: null,
                PassiveCost: null,
                UpgradedPassiveCost: null,
                ActiveCost: null,
                UpgradedActiveCost: null,
                UltimateCost: null,
                UpgradedUltimateCost: null,
                Keywords: Keyword.None,
                UpgradedKeywords: Keyword.None,
                EmptyDescription: false,
                RelativeKeyword: Keyword.None,
                UpgradedRelativeKeyword: Keyword.None,
                RelativeEffects: new List<string>(),
                UpgradedRelativeEffects: new List<string>(),
                RelativeCards: new List<string>() { nameof(Knife) },
                UpgradedRelativeCards: new List<string>() { nameof(Knife) },
                Owner: "Sakuya",
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false, 
                Illustrator: "@kon_ypaaa",
                SubIllustrator: new List<string>()
                );
        }
    }

    [EntityLogic(typeof(ConsequenceOfHickeysCardDef))]
    public sealed class ConsequenceOfHickeysCard : Card
    {
        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return PerformAction.Gun(Battle.Player, Battle.Player, "ESakuyaShoot2");
			yield return SacrificeAction(Value1);
            yield return AttackAction(selector);
            yield return new AddCardsToHandAction(Library.CreateCards<Knife>(Value2, upgraded: false));
        }
    }
}
