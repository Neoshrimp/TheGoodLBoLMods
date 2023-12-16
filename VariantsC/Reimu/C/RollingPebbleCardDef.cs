using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader;
using System.Collections.Generic;
using static VariantsC.BepinexPlugin;
using LBoL.EntityLib.Adventures.Shared12;

namespace VariantsC.Reimu.C
{

    public sealed class RollingPebbleCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(RollingPebbleCard);

        public override CardImages LoadCardImages()
        {
            var ci = new CardImages(embeddedSource);
            ci.AutoLoad(this, ".png");
            return ci;
        }

        public override LocalizationOption LoadLocalization()
        {
            var gl = new GlobalLocalization(embeddedSource);
            gl.LocalizationFiles.AddLocaleFile(Locale.En, "CardsEn");
            return gl;
        }

        public override CardConfig MakeConfig()
        {
            return new CardConfig(
                Index: 0,
                Id: "",
                Order: 10,
                AutoPerform: true,
                Perform: new string[0][],
                GunName: "ENitoriShoot1",
                GunNameBurst: "",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: poolNewCommons.Value,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Common,
                Type: CardType.Attack,
                TargetType: TargetType.SingleEnemy,
                Colors: new List<ManaColor>() { ManaColor.Colorless },
                IsXCost: false,
                Cost: new ManaGroup() { Any = 1 },
                UpgradedCost: null,
                MoneyCost: null,
                Damage: 6,
                UpgradedDamage: null,
                Block: null,
                UpgradedBlock: null,
                Shield: null,
                UpgradedShield: null,
                Value1: 5,
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
                Keywords: Keyword.Synergy,
                UpgradedKeywords: Keyword.Synergy | Keyword.Accuracy,
                EmptyDescription: false,
                RelativeKeyword: Keyword.None,
                UpgradedRelativeKeyword: Keyword.None,
                RelativeEffects: new List<string>(),
                UpgradedRelativeEffects: new List<string>(),
                RelativeCards: new List<string>(),
                UpgradedRelativeCards: new List<string>(),
                Owner: "Koishi",
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false,
                Illustrator: "わらび餅",
                SubIllustrator: new List<string>() { "minato_hitori" }
                );
        }
    }


    [EntityLogic(typeof(RollingPebbleCardDef))]
    public sealed class RollingPebbleCard : Card
    {
        int extraDmg = 0;


        protected override void OnLeaveBattle()
        {
            extraDmg = 0;
        }

        private DamageInfo CalcDmg() => DamageInfo.Attack(RawDamage + extraDmg, IsAccuracy);

        public override DamageInfo Damage => CalcDmg();

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {

            if (IsUpgraded)
                extraDmg += Value1 * SynergyAmount(consumingMana, ManaColor.Any, 1);

            var gun = GunName;
            if (Damage.Amount >= 30)
                gun = "ENitoriShoot2";
            yield return AttackAction(selector.GetUnits(Battle), gunName: gun, damage: CalcDmg());

            if (!IsUpgraded)
                extraDmg += Value1 * SynergyAmount(consumingMana, ManaColor.Any, 1);

        }
    }






}
