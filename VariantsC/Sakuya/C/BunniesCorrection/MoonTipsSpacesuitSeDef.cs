using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.StatusEffects;
using LBoL.Core;
using LBoL.EntityLib.StatusEffects.Basic;
using LBoL.Presentation;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader;
using System;
using System.Collections.Generic;
using System.Text;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Base.Extensions;

namespace VariantsC.Sakuya.C.BunniesCorrection
{
    public sealed class MoonTipsSpacesuitSeDef : StatusEffectTemplate
    {
        public override IdContainer GetId() => nameof(MoonTipsSpacesuitSe);


        public override LocalizationOption LoadLocalization() => BepinexPlugin.StatusEffectBatchLoc.AddEntity(this);

        public override Sprite LoadSprite() => ResourcesHelper.TryGetSprite<Exhibit>(nameof(Yuhangfu));


        public override StatusEffectConfig MakeConfig()
        {
            return new StatusEffectConfig(
                Index: 0,
                Id: "",
                Order: 10,
                Type: StatusEffectType.Positive,
                IsVerbose: false,
                IsStackable: true,
                StackActionTriggerLevel: null,
                HasLevel: false,
                LevelStackType: StackType.Add,
                HasDuration: false,
                DurationStackType: StackType.Add,
                DurationDecreaseTiming: DurationDecreaseTiming.Custom,
                HasCount: false,
                CountStackType: StackType.Keep,
                LimitStackType: StackType.Keep,
                ShowPlusByLimit: false,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>(),
                VFX: "Default",
                VFXloop: "Default",
                SFX: "Default");
        }

    }

    [EntityLogic(typeof(MoonTipsSpacesuitSeDef))]
    public sealed class MoonTipsSpacesuitSe : StatusEffect
    {
        protected override void OnAdded(Unit unit)
        {

            HandleOwnerEvent(Owner.DamageTaking, OnPlayerDamageTaking);
        }

        private void OnPlayerDamageTaking(DamageEventArgs args)
        {
            if (args.DamageInfo.Damage.RoundToInt() > 0)
            {
                base.NotifyActivating();
                args.DamageInfo = args.DamageInfo.ReduceActualDamageBy(1);
                args.AddModifier(this);
            }
        }
    }
}
