﻿using LBoL.Base;
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

namespace VariantsC.Sakuya.C
{
    public sealed class BatFormUltDef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(BatFormUlt);

        public override LocalizationOption LoadLocalization() => BepinexPlugin.UltimateSkillBatchLoc.AddEntity(this);

        public override Sprite LoadSprite() => ResourceLoader.LoadSprite("BatFormUlt.png",BepinexPlugin.embeddedSource);

        public override UltimateSkillConfig MakeConfig()
        {
            return new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 100,
                PowerPerLevel: 100,
                MaxPowerLevel: 2,
                RepeatableType: UsRepeatableType.OncePerTurn,
                Damage: 0,
                Value1: 2,
                Value2: 2,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { nameof(BloodDrainSe) },
                RelativeCards: new List<string>() { nameof(Knife) }
            );
        }
    }

    [EntityLogic(typeof(BatFormUltDef))]
    public sealed class BatFormUlt : UltimateSkill
    {
        public BatFormUlt() 
        {
            TargetType = TargetType.Self;
        }

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {
            //yield return PerformAction.Sfx("Wolf");
            yield return PerformAction.Spell(Owner, Id);
            yield return PerformAction.Effect(Owner, "MoonR", sfxId: "Wolf");

            yield return new ApplyStatusEffectAction<BloodDrainSe>(Owner, level: Value1, duration: 1);
            yield return new AddCardsToHandAction(Library.CreateCards<Knife>(Value2, upgraded: false));
        }
    }
}
