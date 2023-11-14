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

namespace FullElite
{
    public abstract class BaseFullEliteJadeBoxDef : JadeBoxTemplate
    {

        public const string JdBoxGroup = "FullElite";

        public override LocalizationOption LoadLocalization() 
        {
            var gl = new GlobalLocalization(BepinexPlugin.embeddedSource);
            gl.LocalizationFiles.AddLocaleFile(LBoL.Core.Locale.En, "JadeboxesEn");
            return gl;
        }

        public override JadeBoxConfig MakeConfig()
        {
            var con = DefaultConfig();
            con.Value1 = 4;
            con.Value2 = 50;
            con.Group = new List<string>() { JdBoxGroup };
            return con;
        }
    }

    public class BaseFullElite : JadeBox
    {
        protected override void OnGain(GameRunController gameRun)
        {

            Func<List<string>, Stage, Stage> poolElites = (List<string> elites, Stage stage) => {

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
            };

            foreach (var s in gameRun.Stages)
            {
                if (s.GetType() == typeof(BambooForest))
                    poolElites(new List<string> { "Sanyue", "Aya", "Rin" }, s);
                else if (s.GetType() == typeof(XuanwuRavine))
                    poolElites(new List<string> { "Nitori", "Youmu", "Kokoro" }, s);
                else if (s.GetType() == typeof(WindGodLake))
                    poolElites(new List<string> { "Clownpiece", "Siji", "Doremy" }, s);
            }

        }

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


                    // 2do queue reward screen if reward screen is showing

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

    public sealed class FullEliteJadeboxDef : BaseFullEliteJadeBoxDef
    {
        public override IdContainer GetId() => nameof(FullElite);

        public override JadeBoxConfig MakeConfig()
        {
            return base.MakeConfig();
        }

        [EntityLogic(typeof(FullEliteJadeboxDef))]
        public sealed class FullElite : BaseFullElite
        {
            protected override void OnGain(GameRunController gameRun)
            {
                base.OnGain(gameRun);
                gameRun.GainPower(Value2);
            }
        }
    }


    public sealed class EzFullEliteJadeboxDef : BaseFullEliteJadeBoxDef
    {
        public override IdContainer GetId() => nameof(EzFullElite);

        [EntityLogic(typeof(EzFullEliteJadeboxDef))]
        public sealed class EzFullElite : BaseFullElite
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
