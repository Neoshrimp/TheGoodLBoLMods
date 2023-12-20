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

namespace VariantsC.Sakuya.C
{
    public sealed class BloodDrainSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId() => nameof(BloodDrainSe);


        public override LocalizationOption LoadLocalization() 
        { 
            var gl = new GlobalLocalization(BepinexPlugin.embeddedSource);
            gl.LocalizationFiles.AddLocaleFile(Locale.En, "StatusEffectsEn");
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
            con.Keywords = Keyword.NaturalTurn;
            con.DurationDecreaseTiming = DurationDecreaseTiming.NormalTurnStart;
            
            return con;
        }
           
    }

    [EntityLogic(typeof(BloodDrainSeDef))]
    public sealed class BloodDrainSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {
            ReactOwnerEvent(Owner.StatisticalTotalDamageDealt, OnStatisticalDamageDealt);
        }

        private IEnumerable<BattleAction> OnStatisticalDamageDealt(StatisticalDamageEventArgs args)
        {

            bool activated = false;
            foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> unitDmgs in args.ArgsTable)
            {
                int totalHeal = 0;
                unitDmgs.Deconstruct(out var unit, out var readOnlyList);
                foreach (DamageEventArgs damageEventArgs in 
                    from ags in readOnlyList
                    where ags.DamageInfo.DamageType == DamageType.Attack
                    select ags into amount
                    where amount.DamageInfo.Damage > 0f
                    select amount)
                    {
                        totalHeal += damageEventArgs.DamageInfo.Damage.ToInt();
                    }
                if (totalHeal > 0)
                {
                    base.NotifyActivating();
                    activated = true;
                    yield return new HealAction(unit, base.Owner, totalHeal, HealType.Vampire, 0f);
                }
            }
            if (activated)
            {
                Level--;
                if(Level <= 0)
                    yield return new RemoveStatusEffectAction(this);
            }
            yield break;
        }
    }
}
