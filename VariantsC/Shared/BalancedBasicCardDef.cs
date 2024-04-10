using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader;
using System;
using System.Collections.Generic;
using System.Text;
using static VariantsC.BepinexPlugin;

namespace VariantsC.Shared
{
    public sealed class BalancedBasicCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(BalancedBasicCard);

        public override CardImages LoadCardImages()
        {
            var ca = new CardImages(embeddedSource);
            ca.AutoLoad(this, ".png");
            return ca;
        }

        public override LocalizationOption LoadLocalization() => CardBatchLoc.AddEntity(this);

        public override CardConfig MakeConfig()
        {
            return new CardConfig(
                Index: 0,
                Id: "",
                Order: 10,
                AutoPerform: true,
                Perform: new string[0][],
                GunName: "ShootC",
                GunNameBurst: "ShootC1",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: false, 
                FindInBattle: true,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Common,
                Type: CardType.Attack,
                TargetType: TargetType.SingleEnemy,
                Colors: new List<ManaColor>(),
                IsXCost: false,
                Cost: new ManaGroup() { Any = 2 },
                UpgradedCost: null,
                MoneyCost: null,
                Damage: 4,
                UpgradedDamage: 7,
                Block: 4,
                UpgradedBlock: 7,
                Shield: null,
                UpgradedShield: null,
                Value1: null,
                UpgradedValue1: null,
                Value2: null,
                UpgradedValue2: null,
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
                Keywords: Keyword.Basic,
                UpgradedKeywords: Keyword.Basic,
                EmptyDescription: false,
                RelativeKeyword: Keyword.None,
                UpgradedRelativeKeyword: Keyword.None,
                RelativeEffects: new List<string>(),
                UpgradedRelativeEffects: new List<string>(),
                RelativeCards: new List<string>(),
                UpgradedRelativeCards: new List<string>(),
                Owner: null,
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false,
                Illustrator: "白Shiro@",
                SubIllustrator: new List<string>());
        }
    }


    [EntityLogic(typeof(BalancedBasicCardDef))]
    public sealed class BalancedBasicCard : Card
    {

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield return DefenseAction(cast: true);
            yield return AttackAction(selector);
        }
    }
}
