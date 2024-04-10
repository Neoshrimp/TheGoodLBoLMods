using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
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
using LBoL.Presentation;
using LBoL.EntityLib.Cards.Enemy;
using UnityEngine;

namespace VariantsC.Sakuya.C.BunniesCorrection
{
    public sealed class MoonTipsSpacesuitCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(MoonTipsSpacesuitCard);

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
                GunName: "Simple1",
                GunNameBurst: "Simple1",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: false, 
                FindInBattle: true,
                HideMesuem: true,
                IsUpgradable: false,
                Rarity: Rarity.Uncommon,
                Type: CardType.Tool,
                TargetType: TargetType.Nobody,
                Colors: new List<ManaColor>() { },
                IsXCost: false,
                Cost: new ManaGroup() { },
                UpgradedCost: null,
                MoneyCost: null,
                Damage: null,
                UpgradedDamage: null,
                Block: null,
                UpgradedBlock: null,
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
                ToolPlayableTimes: 1,
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
                Owner: null,
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false,
                Illustrator: "DD/neo",
                SubIllustrator: new List<string>()
                );
        }
    }

    [EntityLogic(typeof(MoonTipsSpacesuitCardDef))]
    public sealed class MoonTipsSpacesuitCard : Card
    { }
}
