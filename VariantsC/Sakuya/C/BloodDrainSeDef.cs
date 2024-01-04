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

namespace VariantsC.Sakuya.C
{
    public sealed class BloodDrainSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId() => nameof(BloodDrainSe);


        public override LocalizationOption LoadLocalization() 
        { 
            var gl = new GlobalLocalization(BepinexPlugin.directorySource);
            //gl.LocalizationFiles.AddLocaleFile(Locale.En, "StatusEffectsEn");
            gl.DiscoverAndLoadLocFiles(this);
            return gl;
        } 


        public override Sprite LoadSprite() => ResourcesHelper.TryGetSprite<StatusEffect>(nameof(Vampire));


        public override StatusEffectConfig MakeConfig()
        {
            var con = StatusEffectConfig.FromId(nameof(Vampire)).Copy();
            con.HasLevel = true;
            con.LevelStackType = StackType.Add;
            con.HasDuration = true;
            con.DurationStackType = StackType.Keep;
            con.Keywords = Keyword.None;
            con.DurationDecreaseTiming = DurationDecreaseTiming.TurnStart;
            
            return con;
        }
           
    }

    [EntityLogic(typeof(BloodDrainSeDef))]
    public sealed class BloodDrainSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            ReactOwnerEvent(Owner.StatisticalTotalDamageDealt, OnStatisticalDamageDealt);
            HandleOwnerEvent(Owner.DamageDealt, OnDamageDealt);
        }

        bool CouldActivate(DamageInfo damageInfo) => damageInfo.DamageType == DamageType.Attack && damageInfo.Amount > 0f;

        Stack<GameEntity> dmgSources = new Stack<GameEntity>();


        void OnDamageDealt(DamageEventArgs args) 
        {
            if (args.Cause != ActionCause.OnlyCalculate
                && CouldActivate(args.DamageInfo)
                && dmgSources.Count < Level
                && !dmgSources.Contains(args.ActionSource)
                )
            {
                dmgSources.Push(args.ActionSource);
            }
        }


        private IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
        {
            bool activated = false;

            if (dmgSources.Peek() != args.ActionSource)
            {
                yield break;
            }

            foreach (var unitDmgs in args.ArgsTable)
            {
                int totalHeal = 0;
                unitDmgs.Deconstruct(out var unit, out var damageEvents);
                foreach (DamageEventArgs dmg in damageEvents)
                {
                    if (CouldActivate(dmg.DamageInfo))
                    {  
                        activated = true;
                        totalHeal += dmg.DamageInfo.Damage.ToInt();
                    }
                }
                if (totalHeal > 0)
                {
                    base.NotifyActivating();
                    yield return new HealAction(unit, base.Owner, totalHeal, HealType.Vampire, 0f);
                }
            }
            if (activated)
            {
                Level--;
                if(Level <= 0)
                    yield return new RemoveStatusEffectAction(this);
            }
            dmgSources.Pop();


        }
    }
}
