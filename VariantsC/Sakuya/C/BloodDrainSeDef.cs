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
            if (base.Battle.BattleShouldEnd)
            {
                yield break;
            }
            bool activated = false;
            int totalHeal = 0;
            foreach (KeyValuePair<Unit, IReadOnlyList<DamageEventArgs>> keyValuePair in args.ArgsTable)
            {
                Unit unit;
                IReadOnlyList<DamageEventArgs> readOnlyList;
                keyValuePair.Deconstruct(out unit, out readOnlyList);
                Unit unit2 = unit;
                foreach (DamageEventArgs damageEventArgs in 
                    from ags in readOnlyList
                    where ags.DamageInfo.DamageType == DamageType.Attack
                    select ags into amount
                    where amount.DamageInfo.Damage > 0f
                    select amount)
                    {
                        totalHeal += damageEventArgs.DamageInfo.Damage.ToInt();
                    }
                if (totalHeal > 0 && !activated)
                {
                    base.NotifyActivating();
                    activated = true;
                    yield return new HealAction(unit2, base.Owner, totalHeal, HealType.Vampire, 0f);
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
