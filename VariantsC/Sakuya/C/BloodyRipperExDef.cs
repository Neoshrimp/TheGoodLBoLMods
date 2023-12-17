using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Exhibits;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static VariantsC.BepinexPlugin;

namespace VariantsC.Sakuya.C
{
    public sealed class BloodyRipperExDef : ExhibitTemplate
    {
        public override IdContainer GetId() => nameof(BloodyRipperEx);


        public override LocalizationOption LoadLocalization() => new GlobalLocalization(embeddedSource);


        public override ExhibitSprites LoadSprite() => new ExhibitSprites(ResourceLoader.LoadSprite("BloodyRipperEx.png", embeddedSource));


        public override ExhibitConfig MakeConfig()
        {
            return new ExhibitConfig(
                Index: 0,
                Id: "",
                Order: 25,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Anywhere,
                Owner: "Sakuya",
                LosableType: ExhibitLosableType.DebutLosable,
                Rarity: Rarity.Shining,
                Value1: 13,
                Value2: 3,
                Value3: null,
                Mana: null,
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Red,
                BaseManaAmount: 1,
                HasCounter: true,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { nameof(BloodDrainSe) },
                RelativeCards: new List<string>() { nameof(Knife) }
                );
        }

    }


    [EntityLogic(typeof(BloodyRipperExDef))]
    public sealed class BloodyRipperEx : ShiningExhibit
    {
        public override void OnGain(PlayerUnit player)
        {
            base.OnGain(player);
            GameRun.GainMaxHp(Value1, triggerVisual: true, stats: false);
        }

        public override void OnLose(PlayerUnit player)
        {
            base.OnLose(player);
            GameRun.LoseMaxHp(Value1, triggerVisual: true);
        }

        protected override void OnEnterBattle()
        {
            ReactBattleEvent(Battle.CardUsed, OnCardUsed);
            HandleBattleEvent(Owner.TurnEnding, (UnitEventArgs args) => { TrackCounter = 0; });
            HandleBattleEvent(Battle.BattleStarting, (GameEventArgs args) => { React(new ApplyStatusEffectAction<BloodyRipperSe>(Owner, level: Value2, count: 0)); }, (GameEventPriority)(-50));
            TrackCounter = 0;
        }

        protected override void OnLeaveBattle()
        {
            TrackCounter = 0;
        }

        int TrackCounter
        {
            get => Counter; set
            {
                Counter = value;
                if (Owner.TryGetStatusEffect<BloodyRipperSe>(out var br))
                    br.Count = Counter;
            }
        }


        IEnumerable<BattleAction> OnCardUsed(CardUsingEventArgs args)
        {
            if (Battle.BattleShouldEnd)
                yield break;

            if (args.Card.CardType == CardType.Attack)
            {
                TrackCounter++;
                if (TrackCounter >= Value2)
                {
                    NotifyActivating();
                    if (Owner.TryGetStatusEffect<BloodyRipperSe>(out var br))
                        br.NotifyActivating();
                    yield return new ApplyStatusEffectAction<BloodDrainSe>(Owner, duration: 1);
                    TrackCounter = 0;
                }
            }

            if (args.Card.Id != nameof(Knife))
                // could use separate rng
                yield return new DamageAction(Owner, Owner, DamageInfo.HpLose(GameRun.BattleRng.NextInt(1, 3), dontBreakPerfect: true), gunName: "Sacrifice");
            yield break;
        }


    }


    public sealed class BloodRipperSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId() => nameof(BloodyRipperSe);


        public override LocalizationOption LoadLocalization() => new GlobalLocalization(embeddedSource);


        public override Sprite LoadSprite() => ResourceLoader.LoadSprite("BloodyRipperEx.png", embeddedSource);


        public override StatusEffectConfig MakeConfig()
        {
            var con = DefaultConfig();
            con.HasLevel = true;
            con.Type = StatusEffectType.Special;
            con.HasCount = true;

            return con;
        }

    }

    [EntityLogic(typeof(BloodRipperSeDef))]
    public sealed class BloodyRipperSe : StatusEffect { }
}
