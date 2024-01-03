using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader;
using System.Collections.Generic;
using static VariantsC.BepinexPlugin;
using LBoL.EntityLib.Exhibits;
using LBoLEntitySideloader.Attributes;
using System.Collections;
using LBoL.Core.Units;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using System.Linq;
using System;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Base.Extensions;

namespace VariantsC.Reimu.C
{
    public sealed class ChocolateCoinExDef : ExhibitTemplate
    {
        public override IdContainer GetId() => nameof(ChocolateCoinEx);


        public override LocalizationOption LoadLocalization() => ExhibitBatchLoc.AddEntity(this);

        public override ExhibitSprites LoadSprite()
        {
            return new ExhibitSprites(ResourceLoader.LoadSprite("ChocolateCoinEx.png", embeddedSource));
        }

        public override ExhibitConfig MakeConfig()
        {
            return new ExhibitConfig(
                Index: 0,
                Id: "",
                Order: -5,
                IsDebug: false,
                IsPooled: false,
                IsSentinel: false,
                Revealable: false,
                Appearance: AppearanceType.Anywhere,
                Owner: "Reimu",
                LosableType: ExhibitLosableType.DebutLosable,
                Rarity: Rarity.Shining,
                Value1: 4,
                Value2: null,
                Value3: null,
                Mana: null,
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Colorless,
                BaseManaAmount: 1,
                HasCounter: true,
                InitialCounter: null,
                Keywords: Keyword.Basic | Keyword.Exile,
                RelativeEffects: new List<string>(),
                RelativeCards: new List<string>());

        }

        [EntityLogic(typeof(ChocolateCoinExDef))]
        public sealed class ChocolateCoinEx : ShiningExhibit
        {
            protected override void OnEnterBattle()
            {
                ReactBattleEvent(Battle.Player.TurnStarting, OnTurnStarting);
                Counter = Value1;
            }
            protected override void OnLeaveBattle()
            {
                Counter = 0;
            }

            IEnumerable<BattleAction> OnTurnStarting(UnitEventArgs args)
            {
                if (Counter > 0)
                {
                    var basics = Battle.DrawZone.Where(c => c.IsBasic);
                    if (basics.Count() == 0)
                        basics = Battle.DiscardZone.Where(c => c.IsBasic);
                    if (basics.Count() == 0)
                        basics = Battle.HandZone.Where(c => c.IsBasic);
                    if (basics.Count() > 0)
                    {
                        NotifyActivating();
                        yield return new ExileCardAction(basics.Sample(GameRun.BattleRng));
                    }

                    Counter--;
                }
                yield break;
            }




        }
    }






}
