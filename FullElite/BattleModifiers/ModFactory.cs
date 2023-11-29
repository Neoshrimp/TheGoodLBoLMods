using FullElite.BattleModifiers.Actions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.StatusEffects;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.Presentation;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoL.Presentation.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FullElite.BattleModifiers
{
    public class ModFactory
    {
        /// <summary>
        /// Avoids freezing the argument during early initialization.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getArg"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static ModMod LazyArg<T>(Func<T> getArg, Func<T, ModMod> target) => (Unit unit) =>
        {
            var arg = getArg();
            return target(arg)(unit);
        };

        static public ModMod AddSE<T>(int? level = null, int? duration = null, int? count = null, int? limit = null, float occupationTime = 0f, bool startAutoDecreasing = true) where T : StatusEffect
            => AddSE(typeof(T), level, duration, count, limit, occupationTime, startAutoDecreasing);

        static public ModMod AddSE(Type se, int? level = null, int? duration = null, int? count = null, int? limit = null, float occupationTime = 0f, bool startAutoDecreasing = true)
        {
            return (Unit unit) => {
                unit.React(new BattleAction[] { new ApplySEnoTriggers(type: se, target: unit, level: level, duration: duration, count: count, limit: limit, occupationTime: occupationTime, startAutoDecreasing: startAutoDecreasing) });

                return unit; 
            };
        }


        static public ModMod MulEffetiveHp(float multiplier)
        {
            return MultiplyHp(multiplier, multiplyBlock: true);
        }

        static public ModMod MultiplyHp(float multiplier, bool multiplyBlock = false) 
        {
            return (Unit unit) => {
                
                var hp = ModifyBlockShield.ApplyMul(unit.MaxHp, multiplier, 1);
                unit.SetMaxHp(hp, hp);

                if(multiplyBlock)
                    unit.React(new ModifyBlockShield(unit, 0, 0, multiplier, forced: true));

                var unitView = (unit.View as UnitView);
                if (unitView != null)
                {
                    unitView._statusWidget.SetHpBar();
                    if (unit is PlayerUnit)
                    {
            			UiManager.GetPanel<SystemBoard>().OnMaxHpChanged();
                        unitView.OnMaxHpChanged();
                    }
                }
                return unit;
            };
        }

        // 2do doesn't work on spawns
        static public ModMod ScaleModel(float multiplier)
        {
            return (Unit unit) => {
                var unitView = (unit.View as UnitView);
                if (unitView != null)
                {
                    unitView.transform.localScale = unitView.transform.localScale * multiplier;
                }
                return unit;
            };
        }

        static public ModMod StageCheck(Type stageType, ModMod mod)
        {
            return (Unit unit) => { if (PrecondFactory.IsStage(stageType)(unit)) return unit; return mod(unit); };
        }

        static public ModMod DoSomeAction(ModMod action)
        {
            return (Unit unit) => { unit.React(new ArbitraryBattleAction(unit, action)); return unit; };
        }


    }
}
