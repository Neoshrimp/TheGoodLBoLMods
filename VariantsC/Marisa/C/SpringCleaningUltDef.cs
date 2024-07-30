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
using VariantsC.Sakuya.C;
using VariantsC.Shared;

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
                MaxPowerLevel: 2,
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

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {
            yield return PerformAction.Spell(Owner, Id);
            yield return PerformAction.Effect(Owner, "MoonG", sfxId: "MarisaBottleLaunch");


            yield return new DiscardManyAction(Battle.HandZone);
            yield return new StackDrawToDiscard();


            yield return new DrawManyCardAction(base.Value1);
            yield return new GainManaAction(Mana);

        }
    }
}
