using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace VariantsC.Sakuya.C
{
    public sealed class ChaoticBloodMagicCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(ChaoticBloodMagicCard);

        public override CardImages LoadCardImages() 
        { 
            var ci = new CardImages(BepinexPlugin.embeddedSource);
            ci.AutoLoad(this, ".png");
            return ci;
        }

        public override LocalizationOption LoadLocalization() => BepinexPlugin.CardBatchLoc.AddEntity(this);

        public override CardConfig MakeConfig() 
        {
            return new CardConfig(
                Index: 0,
                Id: "",
                Order: 10,
                AutoPerform: true,
                Perform: new string[0][],
                GunName: "RemiZhua", // RemiZhua
                GunNameBurst: "RemiZhuaB",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: true,
                FindInBattle: true,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Common,
                Type: CardType.Attack,
                TargetType: TargetType.AllEnemies,
                Colors: new List<ManaColor>() { ManaColor.Red } ,
                IsXCost: false,
                Cost: new ManaGroup() { Any = 1, Red = 1 },
                UpgradedCost: null,
                MoneyCost: null,
                Damage: 11,
                UpgradedDamage: 17,
                Block: null,
                UpgradedBlock: null,
                Shield: null,
                UpgradedShield: null,
                Value1: null,
                UpgradedValue1: null,
                Value2: null,
                UpgradedValue2: null,
                Mana: new ManaGroup() { Red = 2 },
                UpgradedMana: new ManaGroup() { Red = 1, Philosophy = 1 },
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
                RelativeCards: new List<string>(),
                UpgradedRelativeCards: new List<string>(),
                Owner: "Sakuya",
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false,
                Illustrator: "かわやばぐ",
                SubIllustrator: new List<string>()
                );
        }
    }

    [EntityLogic(typeof(ChaoticBloodMagicCardDef))]
    public sealed class ChaoticBloodMagicCard : Card
    {

        protected override void OnEnterBattle(BattleController battle)
        {
			ReactBattleEvent(Battle.EnemyDied, OnEnemyDied);

        }


        private IEnumerable<BattleAction> OnEnemyDied(DieEventArgs args)
        {
            if (args.DieSource == this)
            {
                yield return new GainManaAction(Mana);
            }
        }
    }
}
