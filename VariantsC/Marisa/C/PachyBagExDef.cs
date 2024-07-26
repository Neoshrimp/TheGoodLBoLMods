using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core;
using LBoL.Core.StatusEffects;
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
using LBoL.Core.Units;
using LBoL.Base.Extensions;
using System.Linq;
using LBoL.Base;
using LBoL.Core.Cards;
using LBoL.Presentation.I10N;
using static VariantsC.BepinexPlugin;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Exhibits;
using static UnityEngine.UI.GridLayoutGroup;
using VariantsC.Sakuya.C;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.Stage2;
using LBoL.EntityLib.Cards.Character.Marisa;
using HarmonyLib;

namespace VariantsC.Marisa.C
{

    public abstract class NodeData
    {
        public abstract string Key { get; }
    }

    public class CardData : NodeData
    {
        public override string Key => nameof(CardData);

        public List<Card> cards = new List<Card>();
    }

    public sealed class PachyBagExDef : ExhibitTemplate
    {
        public override IdContainer GetId() => nameof(PachyBagEx);


        public override LocalizationOption LoadLocalization() => ExhibitBatchLoc.AddEntity(this);


        public override ExhibitSprites LoadSprite() => new ExhibitSprites(ResourceLoader.LoadSprite("PachyBagEx.png", embeddedSource));


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
                Owner: "Marisa",
                LosableType: ExhibitLosableType.CantLose,
                Rarity: Rarity.Shining,
                Value1: 30,
                Value2: 2,
                Value3: null,
                Mana: null,
                BaseManaRequirement: null,
                BaseManaColor: ManaColor.Green,
                BaseManaAmount: 1,
                HasCounter: true,
                InitialCounter: null,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { nameof(CollectionDefense) }
                );
        }

    }


    [EntityLogic(typeof(PachyBagExDef))]
    public sealed class PachyBagEx : ShiningExhibit
    {
        public int ExtraMana 
        {
            get
            {
                return GameRun.BaseDeck.Count / Value1;
            }
                
        }





        protected override void OnAdded(PlayerUnit player)
        {
            HandleGameRunEvent(GameRun.DeckCardsAdded, new GameEventHandler<CardsEventArgs>(UpdateCounter));
            HandleGameRunEvent(GameRun.DeckCardsRemoved, new GameEventHandler<CardsEventArgs>(UpdateCounter));


            HandleGameRunEvent(GameRun.StationRewardGenerating, delegate (StationEventArgs args)
            {
                Station station = args.Station;

                if (station.Type == StationType.Enemy)
                {
                    NotifyActivating();
                    station.Rewards.Add(station.Stage.GetEnemyCardReward());
                }
                else if (station.Type == StationType.EliteEnemy || (station is AdventureStation adv && adv.Adventure is YachieOppression))
                {
                    NotifyActivating();
                    station.Rewards.Add(station.Stage.GetEliteEnemyCardReward());
                }

                foreach (var r in station.Rewards.FindAll(re => re.Cards?.Count >= 2))
                {
                    var count = r.Cards.Count;

                    var potentialIdexes = Enumerable.Range(0, Math.Max(10, count)).ToList();
                    var subRng = new RandomGen(GameRun.GameRunEventRng.NextULong());
                    potentialIdexes.Shuffle(subRng);

                    var toRemove = new HashSet<Card>();
                    int amountToRemove = count - Value2 < 1 ? 1 : Value2;
                    foreach (var i in potentialIdexes)
                    {
                        if (i >= count)
                            continue;
                        toRemove.Add(r.Cards[i]);
                        if (toRemove.Count >= amountToRemove)
                            break;
                    }

                    r.Cards.RemoveAll(c => toRemove.Contains(c));

                }
            }, (GameEventPriority)(99));


            CardConfig.FromId(nameof(CollectionDefense)).IsUpgradable = false;
            GameRun.BaseDeck.Where(c => c.Id == nameof(CollectionDefense) && c.IsUpgraded).Do(c => c.IsUpgraded = false );
        }

        // stinky
        [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.LeaveGameRun))]
        class GameRunController_Patch
        {
            static void Prefix()
            {
                CardConfig.FromId(nameof(CollectionDefense)).IsUpgradable = true;
            }
        }


        protected override void OnRemoved(PlayerUnit player)
        {
            CardConfig.FromId(nameof(CollectionDefense)).IsUpgradable = true;
        }

        private void UpdateCounter(CardsEventArgs args)
        {
            int prevCounter = Counter;
            Counter = ExtraMana;
            if (Counter != prevCounter)
                NotifyActivating();
        }

        protected override void OnEnterBattle()
        {
            ReactBattleEvent(Owner.TurnStarted, new EventSequencedReactor<UnitEventArgs>(OnOwnerTurnStarted));

            
        }

        private IEnumerable<BattleAction> OnOwnerTurnStarted(UnitEventArgs args)
        {
            for (int i = 0; i < ExtraMana; i++)
            {
                NotifyActivating();
                ManaGroup manaGroup = ManaGroup.Single(ManaColors.Colors.Sample(GameRun.BattleRng));
                yield return new GainManaAction(manaGroup);
            }
        }


    }
}
