﻿using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActionRecord;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VariantsC.Rng;
using VariantsC.Shared;

namespace VariantsC.Marisa.C
{
    public sealed class EverHoardingCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(EverHoardingCard);

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
                Perform: new string[0][],//new string[][] { new string[]{ "2", "MoonG" }, new string[] { "3", "spell" }, new string[] { "4", "MoonG" } },
                GunName: "", // 
                GunNameBurst: "",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: true,
                FindInBattle: false,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Rare,
                Type: CardType.Ability,
                TargetType: TargetType.Nobody,
                Colors: new List<ManaColor>() { ManaColor.Green },
                IsXCost: false,
                Cost: new ManaGroup() { Green = 1},
                UpgradedCost: new ManaGroup() { Green = 1, Any = 2 },
                Kicker: null,
                UpgradedKicker: null,
                MoneyCost: null,
                Damage: null,
                UpgradedDamage: null,
                Block: null,
                UpgradedBlock: null,
                Shield: null,
                UpgradedShield: null,
                Value1: 3,
                UpgradedValue1: 2,
                Value2: null,
                UpgradedValue2: null,
                Mana: new ManaGroup() { Any = 1 },
                UpgradedMana: null,
                Scry: null,
                UpgradedScry: null,
                ToolPlayableTimes: null,
                Loyalty: null,
                UpgradedLoyalty: null,
                PassiveCost: null,
                UpgradedPassiveCost: null,
                ActiveCost: null,
                ActiveCost2: null,
                UpgradedActiveCost2: null,
                UpgradedActiveCost: null,
                UltimateCost: null,
                UpgradedUltimateCost: null,
                Keywords: Keyword.Retain,
                UpgradedKeywords: Keyword.Retain | Keyword.Initial,
                EmptyDescription: false,
                RelativeKeyword: Keyword.None,
                UpgradedRelativeKeyword: Keyword.None,
                RelativeEffects: new List<string>(),
                UpgradedRelativeEffects: new List<string>(),
                RelativeCards: new List<string>(),
                UpgradedRelativeCards: new List<string>(),
                Owner: "Marisa",
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false,
                Illustrator: "porokin",
                SubIllustrator: new List<string>()
                );
        }
    }

    [EntityLogic(typeof(EverHoardingCardDef))]
    public sealed class EverHoardingCard : Card
    {


        public override void Initialize()
        {
            base.Initialize();
            CardCounter = Value1;
        }

        protected override void OnEnterBattle(BattleController battle)
        {
            CardCounter = Value1;
            HandleBattleEvent(Battle.CardUsed, args => {
                if (base.Zone == CardZone.Hand)
                { 
                    CardCounter--;
                    NotifyChanged();
                    if (CardCounter <= 0)
                    { 
                        IncreaseBaseCost(Mana);
                        CardCounter = Value1;
                        NotifyActivating();
                    }
                
                }
            });
        }

        public override void Upgrade()
        {
            base.Upgrade();
            if (Battle == null)
                CardCounter = Value1;
            NotifyChanged();
        }


        private int _cardCounter = 0;

        public int CardCounter { get => _cardCounter; set => _cardCounter = value % (Value1+1); }
        public string CardCounterWrap { get => Battle == null ? "" : $"{{<color=#B2FFFF>{CardCounter}</color>}} "; }

        public string JustName { get => base.LocalizeProperty("JustName", false, true); }

        public string JustNameBlue { get => StringDecorator.GetEntityName(JustName); }

        public override string Name => base.Name.RuntimeFormat(FormatWrapper);

        protected override IReadOnlyList<string> LocalizeListProperty(string key, bool required = true)
        {
            return base.LocalizeListProperty(key, required);
        }

        public readonly float rareWMult = 1.15f;
        public readonly float uncommonWMult = 0.95f;

        private RarityWeightTable GetDynamicRarityTable(int amount)
        {

            double totalW = 1f;
            //var rareW = Math.Clamp(totalW * (amount / MaxGenf) * rareWMult, 0f, 1f);
            //var rareW = Math.Clamp(totalW * (1/(1+Math.Exp(-0.4*(amount-11)))), 0f, 1f);
            var rareW = Math.Clamp(totalW * (1 / (1 + Math.Exp(-0.3 * (amount - 11)))), 0f, 0.7f);


            totalW -= rareW;
            var unCommonW = Math.Clamp(totalW * (amount / 9f) * 0.9f, 0f, totalW);
            totalW -= unCommonW;
            var commonW = Math.Clamp(totalW, 0f, 1f);

            return new RarityWeightTable(commonW.ToFloat(), unCommonW.ToFloat(), rareW.ToFloat(), 0f);
        }


        int GetRarityX(int x)
        { 
            return x - 1;
        }

        public int MaxGen { get => 13; }

        public int _GenAmount(int x) => Math.Clamp(x - 1, 0, MaxGen);
        public int GenAmount { get { return _GenAmount(Cost.Amount); } }

        public string CommonColor { get => CardColors.common; }

        public string CommonChance
        {
            get
            {
                var rwt = GetDynamicRarityTable(GetRarityX(Cost.Amount));
                return $"{rwt.Common:F2}";
            }
        }
        public string UncommonColor { get => CardColors.uncommon; }

        public string UncommonChance
        {
            get
            {
                var rwt = GetDynamicRarityTable(GetRarityX(Cost.Amount));
                return $"{rwt.Uncommon:F2}";
            }
        }


        public string RareColor { get => CardColors.rare; }

        public string RareChance
        {
            get
            {
                var rwt = GetDynamicRarityTable(GetRarityX(Cost.Amount));
                return $"{rwt.Rare:F2}";
            }
        }


        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            int genAmount = _GenAmount(consumingMana.Amount);
            if (genAmount > 0)
            {
                var cardWTable = new CardWeightTable(GetDynamicRarityTable(GetRarityX(consumingMana.Amount)), OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot);

                var cards = GameRun.RollCards(PerRngs.Get(GameRun).everHoardingRng, cardWTable, genAmount, false, true);

                yield return new ProcessDeckCardsAction(cards, cards => GameRun.AddDeckCards(cards, true), "Gained")
                { batchSize = 7, batchDelay = 0.35f, finalDelay = 0f};
            }
            yield break;
        }



        [HarmonyPatch(typeof(Card), nameof(Card.Verify))]
        class DoNotVerify_Patch
        {
            static bool Prefix(Card __instance)
            {
                if (__instance is EverHoardingCard)
                    return false;
                return true;
            }
        }


    }
}
