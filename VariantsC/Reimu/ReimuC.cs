using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoL.Core;
using LBoL.Presentation;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static VariantsC.BepinexPlugin;
using LBoL.EntityLib.Exhibits;
using HarmonyLib;

namespace VariantsC.Reimu
{

    public sealed class JabReimuCUltDef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(JabReimuCUlt);

        public override LocalizationOption LoadLocalization()
        {
            var gl = new GlobalLocalization(embeddedSource);
            gl.LocalizationFiles.AddLocaleFile(Locale.En, "UltimateSkillEn");
            return gl;
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("reimu_fist.png", embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            var config = new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 45,
                PowerPerLevel: 100,
                MaxPowerLevel: 2,
                RepeatableType: UsRepeatableType.FreeToUse,
                Damage: 13,
                Value1: 0,
                Value2: 0,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
                );

            return config;
        }
    }

    [EntityLogic(typeof(JabReimuCUltDef))]
    public sealed class JabReimuCUlt : UltimateSkill
    {
        public JabReimuCUlt()
        {
            base.TargetType = TargetType.SingleEnemy;
            base.GunName = "博丽一拳";
        }

        // remove accurate
        public override DamageInfo Damage => DamageInfo.Attack(Config.Damage);


        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {

            yield return new DamageAction(base.Owner, selector.GetEnemy(base.Battle), this.Damage, base.GunName, GunType.Single);
        }

    }


    public sealed class CounterfeitCoinExDef : ExhibitTemplate
    {
        public override IdContainer GetId() => nameof(CounterfeitCoinEx);


        public override LocalizationOption LoadLocalization()
        {
            var gl = new GlobalLocalization(embeddedSource);
            gl.LocalizationFiles.AddLocaleFile(Locale.En, "ExhibitsEn");
            return gl;
        }

        public override ExhibitSprites LoadSprite()
        {
            return null;
        }

        public override ExhibitConfig MakeConfig()
        {
            return new ExhibitConfig(
                Index: 0,
                Id: "",
                Order: 10,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Anywhere,
                Owner: "Reimu",
                LosableType: ExhibitLosableType.CantLose,
                Rarity: Rarity.Shining,
                Value1: 4,
                Value2: null,
                Value3: null,
                Mana: null,
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Colorless,
                BaseManaAmount: 1,
                HasCounter: false,
                InitialCounter: null,
                Keywords: Keyword.Basic,
                RelativeEffects: new List<string>(),
                RelativeCards: new List<string>());

        }
    }

    [EntityLogic(typeof(CounterfeitCoinExDef))]
    public sealed class CounterfeitCoinEx : ShiningExhibit
    {
    
    }


    public sealed class RollingPebbleCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(RollingPebbleCard);

        public override CardImages LoadCardImages()
        {
            return null;
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
                GunName: "",
                GunNameBurst: "",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: true,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Common,
                Type: CardType.Attack,
                TargetType: TargetType.SingleEnemy,
                Colors: new List<ManaColor>() ,
                IsXCost: true,
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
                Owner: null,
                Unfinished: false,
                Illustrator: null,
                SubIllustrator: new List<string>());
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

        public override ManaGroup GetXCostFromPooled(ManaGroup pooledMana)
        {

            return new ManaGroup() { Colorless = pooledMana.Colorless, Philosophy = pooledMana.Philosophy };
        }

        private DamageInfo CalcDmg() => DamageInfo.Attack((float)(RawDamage + extraDmg), IsAccuracy);

        public override DamageInfo Damage => CalcDmg();

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            
            if (!this.IsUpgraded)
                yield return AttackAction(selector, damage: CalcDmg());

            extraDmg += Value1 * (SynergyAmount(consumingMana, ManaColor.Any, 1));

            if (this.IsUpgraded)
                yield return AttackAction(selector, damage: CalcDmg());

        }
    }



    public sealed class BalancedBasicCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(BalancedBasicCard);

        public override CardImages LoadCardImages()
        {
            var ca = new CardImages(embeddedSource);
            ca.AutoLoad(this, ".png");
            return ca;
        }

        public override LocalizationOption LoadLocalization() => new GlobalLocalization(embeddedSource);

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
