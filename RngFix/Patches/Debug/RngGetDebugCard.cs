using Cysharp.Threading.Tasks;
using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Presentation;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader.Utils;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.Patches.Debug
{
    public /*sealed*/ class RngGetDebugCardDef : CardTemplate
    {
        public override IdContainer GetId() => nameof(RngGetDebugCard);

        public override CardImages LoadCardImages() => null;

        public override LocalizationOption LoadLocalization() => new DirectLocalization(new Dictionary<string, object>() { { "Name", "deez" },
            { "Description", "Sus:{ShouldSuspend} ({SuspendCost})\nNat:{NaturalCost}\nActual:{Cost}" },
        });

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
                FindInBattle: true,
                HideMesuem: false,
                IsUpgradable: true,
                Rarity: Rarity.Common,
                Type: CardType.Skill,
                TargetType: TargetType.Self,
                Colors: new List<ManaColor>() { ManaColor.Red },
                IsXCost: false,
                Cost: new ManaGroup() { Any = 2, Black = 2},
                UpgradedCost: null,
                Kicker: null,
                UpgradedKicker: null,
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
                ToolPlayableTimes: null,
                Loyalty: null,
                UpgradedLoyalty: null,
                PassiveCost: null,
                UpgradedPassiveCost: null,
                ActiveCost: null,
                UpgradedActiveCost: null,
                ActiveCost2: null,
                UpgradedActiveCost2: null,
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
                Illustrator: null,
                SubIllustrator: new List<string>());
        }

        public static RandomGen StaticRng() => GrRngs.Gr()?.BattleRng;

        // ignore
        //[HarmonyPatch(typeof(Card), nameof(Card.Cost), MethodType.Getter)]
        /*            class Cost_Patch
                    {
                        static bool Prefix(Card __instance, ref ManaGroup __result)
                        {
                            if (__instance is RngGetDebugCard susCard)
                            {
                                if (susCard.ShouldSuspend)
                                    __result = susCard.ForceCost;
                                else
                                    __result = susCard.NaturalCost;
                                return false;
                            }

                            return true;
                        }
                    }*/


        //[EntityLogic(typeof(RngGetDebugCardDef))]
        public /*sealed*/ class RngGetDebugCard : Card
        {

            ManaGroup suspendCost = new ManaGroup() { Any = 1, Black = 1 };
            public ManaGroup SuspendCost { get => suspendCost; }

            bool shouldSuspend = false;
            public bool ShouldSuspend { get => shouldSuspend; set => shouldSuspend = value; }



            public ManaGroup NaturalCost
            {
                get
                {
                    if (this.IsXCost)
                    {
                        return this.BaseCost;
                    }
                    if (this.FreeCost || this.Summoned)
                    {
                        return ManaGroup.Empty;
                    }
                    ManaGroup manaGroup = (this.TurnCost + this.AdditionalCost + this.AuraCost).Corrected;
                    manaGroup = (this.IsPurified ? manaGroup.Purified() : manaGroup);
                    BattleController battle = this.Battle;
                    if (battle != null && battle.ManaFreezeLevel > 0 && this.Zone == CardZone.Hand)
                    {
                        return manaGroup + ManaGroup.Anys(this.Battle.ManaFreezeLevel);
                    }
                    return manaGroup;
                }
            }

            public override ManaGroup ForceCost => SuspendCost;

            public override bool IsForceCost 
            {
                get 
                {
                    if (Battle != null && !Battle.BattleMana.CanAfford(NaturalCost))
                    {
                        ShouldSuspend = true;
                        // probably ok to change target type here
                        NotifyChanged();
                        return true;
                    }
                    ShouldSuspend = false;
                    // probably ok to change target type here
                    NotifyChanged();
                    return false;
                }
            }


            protected override void OnEnterBattle(BattleController battle)
            {
                base.OnEnterBattle(battle);
/*                Func<RandomGen> ac = () => GameRun.BattleRng;
                log.LogDebug("ac");
                ac();
                Func<RandomGen> acac = () => ac();
                log.LogDebug("acac");
                acac();
                Func<RandomGen> acacac = () => acac();
                log.LogDebug("acacac");
                acacac();
                log.LogDebug("static");
                RngGetDebugCardDef.StaticRng();
                log.LogDebug("async");
                GameMaster.Instance.StartCoroutine(AsyncRng().ToCoroutine());*/

            }

            UniTask<RandomGen> AsyncRng() => UniTask.RunOnThreadPool(() => GameRun.BattleRng);

            protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
            {

                log.LogDebug($"suspend deez: {ShouldSuspend}");


/*                Func<RandomGen> ac = () => GameRun.BattleRng;
                log.LogDebug("Yield_ac");
                ac();
                Func<RandomGen> acac = () => ac();
                log.LogDebug("Yield_acac");
                acac();
                Func<RandomGen> acacac = () => acac();
                log.LogDebug("Yield_acacac");
                acacac();
                log.LogDebug("Yield_static");
                RngGetDebugCardDef.StaticRng();
                log.LogDebug("Yield_async");
                GameMaster.Instance.StartCoroutine(AsyncRng().ToCoroutine());*/
                yield break;
            }
        }
    }


    
}
