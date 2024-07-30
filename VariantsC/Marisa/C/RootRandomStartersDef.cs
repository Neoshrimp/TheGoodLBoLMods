using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Helpers;
using LBoL.Core.Randoms;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using VariantsC.Rng;
using static VariantsC.BepinexPlugin;

namespace VariantsC.Marisa.C
{
    public abstract class RootRandomStartersDef : CardTemplate
    {

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
                IsPooled: false,
                FindInBattle: false,
                HideMesuem: true,
                IsUpgradable: false,
                Rarity: Rarity.Common,
                Type: CardType.Skill,
                TargetType: TargetType.Nobody,
                Colors: new List<ManaColor>() { ManaColor.Colorless },
                IsXCost: false,
                Cost: new ManaGroup() { Colorless = 1 },
                UpgradedCost: null,
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
                Mana: new ManaGroup() { },
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
                Owner: "Marisa",
                ImageId: "",
                UpgradeImageId: "",
                Unfinished: false,
                Illustrator: "",
                SubIllustrator: new List<string>()
                );
        }
    }

    public class RootRandomStarter : Card
    {
        public virtual RandomPoolEntry<Type[]>[] PotentialCardTypes { get; }

        public string ListCards
        {
            get
            {
                return string.Join(";\n", PotentialCards().Select(rp => string.Join(", ", rp.Elem.Select(c => UiUtils.WrapByColor(c.Name, GlobalConfig.DefaultKeywordColor))))) + ";";
            }
        }

        public static RandomGen transformRng = null;


        public string AddTimesWrap { get => AddTimes <= 1 ? "" : $" <b>{{X{AddTimes}}}</b>"; }
        public virtual int AddTimes { get => 1; }

        public override void Initialize()
        {
            base.Initialize();
            Config.RelativeCards = new List<string>(PotentialCardTypes.SelectMany(rp => rp.Elem).Select(t => t.Name).Distinct());
        }

        

        public IEnumerable<RandomPoolEntry<List<Card>>> PotentialCards(GameRunController gr = null)
        {
            return PotentialCardTypes.Select(rp =>
            {
                var l = new List<Card>();
                foreach (var t in rp.Elem)
                { 
                    var c = Library.CreateCard(t);
                    c.GameRun = gr;
                    l.Add(c);
                }
                return new RandomPoolEntry<List<Card>>(l, rp.Weight);
            });
        }

        public string JustName { get => base.LocalizeProperty("JustName", false, true); }

        public string JustNameBlue { get => StringDecorator.GetEntityName(JustName); }

        public override string Name => base.Name.RuntimeFormat(FormatWrapper);

        protected override string LocalizeProperty(string key, bool decorated = false, bool required = true)
        {
            if (key == "Description")
                return TypeFactory<Card>.LocalizeProperty(nameof(FakePackForCommonLoc), key, decorated, required);
            if (key == "Name")
                return TypeFactory<Card>.LocalizeProperty(nameof(FakePackForCommonLoc), "Name", decorated, required);
            return base.LocalizeProperty(key, decorated, required);
        }

        public void OnGrStarted(GameRunController gr)
        {
            gr.RemoveDeckCard(this, false);
            if (transformRng == null)
                transformRng = new RandomGen(gr.CardRng.NextULong());

            var pool = new RepeatableRandomPool<List<Card>>();
            pool.AddRange(PotentialCards(gr));

            for(int i = 0; i < AddTimes; i++)
                gr.AddDeckCards(pool.Sample(transformRng), false);
        }

        protected override IEnumerable<BattleAction> Actions(UnitSelector selector, ManaGroup consumingMana, Interaction precondition)
        {
            yield break;
        }
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.VeryLow)]
    class GrStartEvents
    {

        public static List<Action<GameRunController>> onGrStarted = new List<Action<GameRunController>>();

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Constructor(typeof(GameRunController), new Type[] { typeof(GameRunStartupParameters) });
        }

        static void Prefix(GameRunStartupParameters startupParameters)
        {
            onGrStarted.Clear();
            RootRandomStarter.transformRng = null;

            startupParameters.Deck.Where(c => c is RootRandomStarter).Cast<RootRandomStarter>().Do(sp => onGrStarted.Add(sp.OnGrStarted));
        }

        static void Postfix(GameRunController __instance)
        {
            onGrStarted.Do(a => a(__instance));
        }


    }


}
