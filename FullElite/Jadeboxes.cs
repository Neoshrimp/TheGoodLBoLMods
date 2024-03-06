using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.UI;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static FullElite.BepinexPlugin;
using LBoL.EntityLib.Stages.NormalStages;
using System.Text.RegularExpressions;
using LBoL.Core.StatusEffects;
using HarmonyLib;
using JetBrains.Annotations;
using System.Linq;
using System.Reflection;
using FullElite.BattleModifiers;
using LBoL.Core.Units;
using LBoL.EntityLib.StatusEffects.Reimu;

namespace FullElite
{
    public abstract class BaseFullEliteJadeBoxDef : JadeBoxTemplate
    {

        public const string JdBoxGroup = "FullElite";

        public override LocalizationOption LoadLocalization() => jadeboxBatchLoc.AddEntity(this);


        public override JadeBoxConfig MakeConfig()
        {
            var con = DefaultConfig();
            con.Group = new List<string>() { JdBoxGroup };
            return con;
        }
    }



    public class BaseFullElite : JadeBox
    {
        protected Stage PoolElites(IEnumerable<string> elites, Stage stage)
        {
            stage.EnemyPoolAct1 = new UniqueRandomPool<string>(true);
            stage.EnemyPoolAct2 = new UniqueRandomPool<string>(true);
            stage.EnemyPoolAct3 = new UniqueRandomPool<string>(true);
            foreach (var e in elites)
            {
                stage.EnemyPoolAct1.Add(e);
                stage.EnemyPoolAct2.Add(e);
                stage.EnemyPoolAct3.Add(e);
            }
            return stage;
        }
      
    }



    public sealed class FullEliteJadeboxDef : BaseFullEliteJadeBoxDef
    {
        public override IdContainer GetId() => nameof(FullEliteBox);


        [EntityLogic(typeof(FullEliteJadeboxDef))]
        public sealed class FullEliteBox : BaseFullElite
        {
            protected override void OnGain(GameRunController gameRun)
            {
                foreach (var s in gameRun.Stages)
                {
                    if (s.GetType() == typeof(BambooForest))
                        PoolElites(new List<string> { "Sanyue", "Aya", "Rin" }, s);
                    else if (s.GetType() == typeof(XuanwuRavine))
                        PoolElites(new List<string> { "Nitori", "Youmu", "Kokoro" }, s);
                    else if (s.GetType() == typeof(WindGodLake))
                        PoolElites(new List<string> { "Clownpiece", "Siji", "Doremy" }, s);
                }
            }
        }

    }


    public sealed class RainbowFullEliteJadeboxDef : BaseFullEliteJadeBoxDef
    {
        public override IdContainer GetId() => nameof(RainbowFullEliteBox);

        [EntityLogic(typeof(RainbowFullEliteJadeboxDef))]
        public sealed class RainbowFullEliteBox : BaseFullElite
        {
            protected override void OnGain(GameRunController gameRun)
            {
                foreach (var s in gameRun.Stages)
                {
                    if (s.GetType() == typeof(BambooForest) || s.GetType() == typeof(XuanwuRavine) || s.GetType() == typeof(WindGodLake))
                    { 
                        PoolElites(VanillaElites.eliteGroups, s);
                        // works because of reference assignments
                        s.EnemyPoolAct2 = s.EnemyPoolAct1;
                        s.EnemyPoolAct3 = s.EnemyPoolAct1;

                        var ogElites = new HashSet<string>();
                        s.EliteEnemyPool.Do(eg => ogElites.Add(eg.Elem));

                        s.EliteEnemyPool = new UniqueRandomPool<string>(true);
                        foreach (var eg in VanillaElites.eliteGroups)
                        {
                            if (ogElites.Contains(eg))
                                s.EliteEnemyPool.Add(eg, 2.1f);
                            else 
                                s.EliteEnemyPool.Add(eg, 1f);
                        }

                    }
                }
            }

            protected override void OnAdded()
            {
                foreach (var s in this.GameRun.Stages)
                {
                    if (s.GetType() == typeof(BambooForest) || s.GetType() == typeof(XuanwuRavine) || s.GetType() == typeof(WindGodLake))
                    {
                        // reassign on game load
                        s.EnemyPoolAct2 = s.EnemyPoolAct1;
                        s.EnemyPoolAct3 = s.EnemyPoolAct1;
                    }
                }


            }

        }


    }




    public sealed class StartingDraftBoxDef : JadeBoxTemplate
    {
        public override IdContainer GetId() => nameof(StartingDraftBox);

        public override LocalizationOption LoadLocalization() => jadeboxBatchLoc.AddEntity(this);

        public override JadeBoxConfig MakeConfig()
        {
            var con = DefaultConfig();
            con.Value1 = 4;
            return con;
        }

        [EntityLogic(typeof(StartingDraftBoxDef))]
        public sealed class StartingDraftBox : JadeBox
        {
            protected override void OnAdded()
            {
                base.HandleGameRunEvent<StationEventArgs>(base.GameRun.StationEntered, delegate (StationEventArgs args)
                {
                    EntryStation entryStation = args.Station as EntryStation;
                    if (entryStation != null && GameRun.Stages.IndexOf(entryStation.Stage) == 0)
                    {
                        var rewards = new List<StationReward>();
                        for (var i = 0; i < Value1; i++)
                        {
                            rewards.Add(GameRun.CurrentStage.GetEnemyCardReward());
                        }
                        entryStation.AddRewards(rewards);



                        UiManager.GetPanel<RewardPanel>().Show(new ShowRewardContent
                        {
                            Station = GameRun.CurrentStation,
                            Rewards = GameRun.CurrentStation.Rewards,
                            ShowNextButton = true
                        });
                    }
                });
            }
        }
    }


    public sealed class PowerBonusBoxDef : JadeBoxTemplate
    {
        public override IdContainer GetId() => nameof(PowerBonusBox);
        public override LocalizationOption LoadLocalization() => jadeboxBatchLoc.AddEntity(this);

        public override JadeBoxConfig MakeConfig()
        {
            var con = DefaultConfig();
            con.Value1 = 50;
            return con;
        }

        [EntityLogic(typeof(PowerBonusBoxDef))]
        public sealed class PowerBonusBox : JadeBox
        {
            protected override void OnGain(GameRunController gameRun)
            {
                gameRun.GainPower(Value1);
            }
        }
    }


    public sealed class BlockToysBonusBoxDef : JadeBoxTemplate
    {
        public override IdContainer GetId() => nameof(BlockToysBonusBox);

        public override LocalizationOption LoadLocalization() => jadeboxBatchLoc.AddEntity(this);


        public override JadeBoxConfig MakeConfig()
        {
            var con = DefaultConfig();
            return con;
        }

        [EntityLogic(typeof(BlockToysBonusBoxDef))]
        public sealed class BlockToysBonusBox : JadeBox
        {
            protected override void OnGain(GameRunController gameRun)
            {
                base.OnGain(gameRun);
                GameMaster.Instance.StartCoroutine(GainExhibits(gameRun, new HashSet<Type>() { typeof(JimuWanju) }));
            }
            private IEnumerator GainExhibits(GameRunController gameRun, HashSet<Type> exhibits)
            {
                foreach (var et in exhibits)
                {
                    yield return gameRun.GainExhibitRunner(Library.CreateExhibit(et));
                }
                gameRun.ExhibitPool.RemoveAll(e => exhibits.Contains(e));
            }
        }

    }


}
