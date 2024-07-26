using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                Perform: new string[0][],
                GunName: "", // 
                GunNameBurst: "",
                DebugLevel: 0,
                Revealable: false,
                IsPooled: BepinexPlugin.poolNewCards.Value,
                FindInBattle: false,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Rare,
                Type: CardType.Skill,
                TargetType: TargetType.Nobody,
                Colors: new List<ManaColor>() { ManaColor.Green },
                IsXCost: false,
                Cost: new ManaGroup() { Green = 1 },
                UpgradedCost: null,
                MoneyCost: null,
                Damage: null,
                UpgradedDamage: null,
                Block: null,
                UpgradedBlock: null,
                Shield: null,
                UpgradedShield: null,
                Value1: 3,
                UpgradedValue1: null,
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
                UpgradedActiveCost: null,
                UltimateCost: null,
                UpgradedUltimateCost: null,
                Keywords: Keyword.Exile | Keyword.Retain,
                UpgradedKeywords: Keyword.Exile | Keyword.Retain | Keyword.Initial,
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
                Illustrator: "",
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
                CardCounter--;
                NotifyChanged();
                if (CardCounter <= 0)
                { 
                    IncreaseBaseCost(Mana);
                    CardCounter = Value1;
                    NotifyActivating();
                }
            });
        }


        private int _cardCounter = 0;

        public int CardCounter { get => _cardCounter; set => _cardCounter = value % (Value1+1); }

        // change trigger condition probably
        public override IEnumerable<BattleAction> OnRetain()
        {
            if (base.Zone == CardZone.Hand)
            {
                base.IncreaseBaseCost(base.Mana);
            }
            yield break;
        }

        public readonly float rareWMult = 1.15f;
        public readonly float uncommonWMult = 0.95f;

        private RarityWeightTable GetDynamicRarityTable(int amount)
        {

            float totalW = 1f;
            var rareW = Math.Clamp(totalW * (amount / 20f) * rareWMult, 0f, 1f);
            totalW -= rareW;
            var unCommonW = Math.Clamp(totalW * (amount / 9f) * uncommonWMult, 0f, 1f);
            totalW -= unCommonW;
            var commonW = Math.Clamp(totalW, 0f, 1f);

            return new RarityWeightTable(commonW, unCommonW, rareW, 0f);
        }

        public string CommonChance
        {
            get
            {
                //string.Join("; ", CardConfig.AllConfig().GroupBy(cc => cc.Rarity).Select(g => $"{g.Key}: {g.Count()}"));
                int amount = Cost.Amount - 1;

                var rwt = GetDynamicRarityTable(amount);

                return $"{rwt.Common:F2}";
            }
        }

        public string UncommonChance
        {
            get
            {
                //string.Join("; ", CardConfig.AllConfig().GroupBy(cc => cc.Rarity).Select(g => $"{g.Key}: {g.Count()}"));
                int amount = Cost.Amount - 1;

                var rwt = GetDynamicRarityTable(amount);

                return $"{rwt.Uncommon:F2}";
            }
        }

        public string RareChance
        {
            get
            {
                //string.Join("; ", CardConfig.AllConfig().GroupBy(cc => cc.Rarity).Select(g => $"{g.Key}: {g.Count()}"));
                int amount = Cost.Amount - 1;

                var rwt = GetDynamicRarityTable(amount);

                return $"{rwt.Rare:F2}";
            }
        }

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            int synergyAmount = Math.Min(consumingMana.Amount - 1, 20);
            if (synergyAmount > 0)
            {
                var cardWTable = new CardWeightTable(GetDynamicRarityTable(synergyAmount), OwnerWeightTable.Hierarchy, CardTypeWeightTable.CanBeLoot);
                // which rng to use
                var cards = GameRun.RollCards(GameRun.BattleCardRng, cardWTable, synergyAmount, false, true);
                GameRun.AddDeckCards(cards, true);
            }
            yield break;
        }

    }
}
