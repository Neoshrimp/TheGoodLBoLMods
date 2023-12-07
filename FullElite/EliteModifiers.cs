using FullElite.BattleModifiers;
using LBoL.Core.StatusEffects;
using LBoL.EntityLib.EnemyUnits.Character.DreamServants;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.Core.Units;
using HarmonyLib;
using LBoL.Presentation;
using LBoL.EntityLib.Stages.NormalStages;
using LBoL.Base;
using LBoL.EntityLib.StatusEffects.Enemy;
using static FullElite.BattleModifiers.ModFactory;
using static FullElite.BattleModifiers.PrecondFactory;
using DG.Tweening;
using LBoL.Core.Battle.BattleActions;
using FullElite.BattleModifiers.Actions;
using LBoL.Presentation.Units;
using UnityEngine;
using LBoL.Core.Battle;

namespace FullElite
{

    public class EliteModifiers
    {

        public static void Innitialize()
        {

            ModPrecond isEliteGroup = (Unit unit) => VanillaElites.eliteGroups.Contains(unit.Battle.EnemyGroup.Id);

            Func<RandomGen> getRng = () => GameMaster.Instance.CurrentGameRun.EnemyBattleRng;
            Func<int, int, Func<int>> nextInt = (int l, int u) => () => getRng().NextInt(l, u);
            Func<float, float, Func<float>> nextFloat = (float l, float u) => () => getRng().NextFloat(l, u);
            Func<Type, Func<int, ModMod>> seWiLevel = (Type se) => (int level) => AddSE(se, level);

            ModPrecond isRainbow = HasJadeboxes(new HashSet<string>() { new RainbowFullEliteJadeboxDef().UniqueId });

            Func<Vector3, ModMod> fairyShenanigans = (Vector3 targetPos) => (Unit unit) => {
                var uv = unit.View as UnitView;
                var scale = UnityEngine.Random.Range(1.5f, 1.85f);
                uv?.transform.DOLocalMove(
                    targetPos + new Vector3(0, UnityEngine.Random.Range(-0.1f, 0.1f)),
                    1.3f).SetEase(Ease.InQuad).OnComplete(() => {
                        uv.PlayEffectOneShot("BuffRed", 0f);
                        AudioManager.PlaySfx("Buff");
                        uv.transform.DOScale(new Vector3(scale, scale), 0.3f);
                    });
                return unit;
            };

            // A1 elites
            foreach (var id in VanillaElites.act1.Select(t => t.Name))
            {
                var a2um = new UnitModifier(id);
                a2um.preconds.Add(isEliteGroup);
                a2um.preconds.Add(isRainbow);
                a2um.preconds.Add(IsStage(typeof(XuanwuRavine)));


                if (!VanillaElites.spirits.Contains(id))
                {
                    a2um.mods.Add(AddSE(typeof(Firepower), 3));
                    a2um.mods.Add(LazyArg(nextFloat(1.7f, 1.9f),  MulEffetiveHp));
                }
                else
                {
                    a2um.mods.Add(LazyArg(nextInt(1, 2), seWiLevel(typeof(Firepower))));
                }
                if (id != nameof(Sunny) && id != nameof(Luna) && id != nameof(Star))
                    a2um.mods.Add(LazyArg(() => UnityEngine.Random.Range(1.2f, 1.4f), ScaleModel));
                if (id == nameof(Aya))
                { 
                    a2um.mods.Add(AddSE<FastAttack>(5));
                    a2um.mods.Add(MultiplyHp(1.15f));
                }


                var a3um = new UnitModifier(id);
                a3um.preconds.Add(isEliteGroup);
                a3um.preconds.Add(isRainbow);
                a3um.preconds.Add(IsStage(typeof(WindGodLake)));

                if (id == nameof(Sunny))
                    a3um.mods.Add(fairyShenanigans(new Vector3(-3, 0)));

                if (id == nameof(Luna))
                    a3um.mods.Add(fairyShenanigans(new Vector3(-2.7f, -1.7f)));

                if (id == nameof(Star))
                    a3um.mods.Add(fairyShenanigans(new Vector3(0, 1.45f)));

                if (!VanillaElites.spirits.Contains(id))
                {
                    if (id != nameof(Sunny) && id != nameof(Luna) && id != nameof(Star))
                        a3um.mods.Add(AddSE(typeof(Firepower), 5));
                    else
                        a3um.mods.Add(AddSE(typeof(Firepower), 6));
                    a3um.mods.Add(LazyArg(nextFloat(2.1f, 2.3f), MulEffetiveHp));
                }
                else
                {
                    a3um.mods.Add(LazyArg(nextFloat(1.05f, 1.2f), MulEffetiveHp));
                    a3um.mods.Add(LazyArg(nextInt(2, 4), seWiLevel(typeof(Firepower))));
                }
                if (id != nameof(Sunny) && id != nameof(Luna) && id != nameof(Star))
                    a3um.mods.Add(LazyArg(() => UnityEngine.Random.Range(1.5f, 1.85f), ScaleModel));
                if (id == nameof(Aya))
                { 
                    a3um.mods.Add(LazyArg(nextInt(7, 11), seWiLevel(typeof(FastAttack))));
                    a3um.mods.Add(LazyArg(nextInt(1, 2), seWiLevel(typeof(WindGirl))));
                    a3um.mods.Add(MultiplyHp(1.2f));
                    a3um.mods.Add(ScaleModel(0.3f));
                }
            }


            // A2 elites
            foreach (var id in VanillaElites.act2.Select(t => t.Name))
            {
                var a2um = new UnitModifier(id);
                a2um.preconds.Add(isEliteGroup);
                a2um.preconds.Add(isRainbow);
                a2um.preconds.Add(IsStage(typeof(BambooForest)));

                if (id == nameof(Nitori))
                {
                    a2um.mods.Add(AddSE(typeof(FirepowerNegative), 4));
                    a2um.mods.Add(LazyArg(nextFloat(0.45f, 0.5f), MulEffetiveHp));
                }
                else
                {
                    a2um.mods.Add(AddSE(typeof(FirepowerNegative), 3));
                    a2um.mods.Add(LazyArg(nextFloat(0.48f, 0.55f), MulEffetiveHp));
                }

                a2um.mods.Add(LazyArg(() => UnityEngine.Random.Range(0.85f, 0.95f), ScaleModel));


                var a3um = new UnitModifier(id);
                a3um.preconds.Add(isEliteGroup);
                a3um.preconds.Add(isRainbow);
                a3um.preconds.Add(IsStage(typeof(WindGodLake)));

                if (id == nameof(Youmu))
                {
                    a3um.mods.Add(AddSE(typeof(LouguanJianSe), 1));
                }
                else
                {
                    a3um.mods.Add(AddSE(typeof(Firepower), 3));
                }
                if (VanillaElites.masks.Contains(id))
                {
                    a3um.mods.Add(LazyArg(nextFloat(1.4f, 1.7f), MulEffetiveHp));
                }
                else
                {
                    a3um.mods.Add(LazyArg(nextFloat(1.7f, 1.9f), MulEffetiveHp));
                    a3um.mods.Add(LazyArg(() => UnityEngine.Random.Range(1.5f, 1.85f), ScaleModel));
                }
            }


            ModMod adjustDoremySleep = (Unit unit) =>
            {
                var doremy = unit as Doremy;
                if (doremy?.Sleep != null)
                {
                    unit.React(new RemoveStatusEffectAction(doremy.Sleep));
                    unit.React(new ApplySEnoTriggers(typeof(Sleep), unit, unit.Shield, count: 3));
                }
                return unit;
            };

            // A3 elites
            foreach (var id in VanillaElites.act3.Select(t => t.Name))
            {
                var a2um = new UnitModifier(id);
                a2um.preconds.Add(isEliteGroup);
                a2um.preconds.Add(isRainbow);
                a2um.preconds.Add(IsStage(typeof(BambooForest)));


                // order matters for Doremy
                a2um.mods.Add(LazyArg(nextFloat(0.28f, 0.33f), MulEffetiveHp));

                if (VanillaElites.eikiSummons.Contains(id))
                    a2um.mods.Add(AddSE(typeof(FirepowerNegative), 4));
                else if (id == nameof(Doremy))
                { 
                    a2um.mods.Add(AddSE(typeof(FirepowerNegative), 7));
                    a2um.mods.Add(DoSomeAction(adjustDoremySleep));
                    a2um.mods.Add(MultiplyHp(0.95f));
                }
                else
                    a2um.mods.Add(AddSE(typeof(FirepowerNegative), 5));

                if (id == nameof(Clownpiece))
                    a2um.mods.Add(MultiplyHp(1.15f));

                if (id == nameof(Doremy))
                    a2um.mods.Add(ScaleModel(0.7f));
                else
                    a2um.mods.Add(LazyArg(() => UnityEngine.Random.Range(0.55f, 0.65f), ScaleModel));



                var a3um = new UnitModifier(id);
                a3um.preconds.Add(isEliteGroup);
                a3um.preconds.Add(isRainbow);
                a3um.preconds.Add(IsStage(typeof(XuanwuRavine)));


                a3um.mods.Add(LazyArg(nextFloat(0.48f, 0.55f), MulEffetiveHp));

                if (VanillaElites.dreamGirls.Contains(id))
                    a3um.mods.Add(AddSE(typeof(FirepowerNegative), 2));
                else if (id == nameof(Doremy))
                {
                    a3um.mods.Add(AddSE(typeof(FirepowerNegative), 5));
                    a3um.mods.Add(DoSomeAction(adjustDoremySleep));
                }
                else
                    a3um.mods.Add(AddSE(typeof(FirepowerNegative), 3));

                if (id == nameof(Clownpiece))
                    a3um.mods.Add(MultiplyHp(1.27f));

                if (id == nameof(Doremy))
                    a2um.mods.Add(ScaleModel(0.95f));
                else
                    a3um.mods.Add(LazyArg(() => UnityEngine.Random.Range(0.88f, 0.95f), ScaleModel));


            }

        }
    }





}
