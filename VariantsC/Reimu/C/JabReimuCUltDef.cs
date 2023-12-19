using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoLEntitySideloader.Entities;
using LBoLEntitySideloader.Resource;
using LBoLEntitySideloader;
using System.Collections.Generic;
using UnityEngine;
using static VariantsC.BepinexPlugin;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Units;
using LBoLEntitySideloader.Attributes;
using VariantsC.Sakuya.C;

namespace VariantsC.Reimu.C
{
    public sealed class JabReimuCUltDef : UltimateSkillTemplate
    {
        public override IdContainer GetId() => nameof(JabReimuCUlt);

        public override LocalizationOption LoadLocalization()
        {
            var gl = new GlobalLocalization(embeddedSource);
            gl.LocalizationFiles.AddLocaleFile(Locale.En, "UltimateSkillEn");
            return gl;
        }

        public override Sprite LoadSprite()
        {
            return ResourceLoader.LoadSprite("reimu_fist.png", embeddedSource);
        }

        public override UltimateSkillConfig MakeConfig()
        {
            var config = new UltimateSkillConfig(
                Id: "",
                Order: 10,
                PowerCost: 45,
                PowerPerLevel: 45,
                MaxPowerLevel: 5,
                RepeatableType: UsRepeatableType.FreeToUse,
                Damage: 13,
                Value1: 0,
                Value2: 0,
                Keywords: Keyword.None,
                RelativeEffects: new List<string>() { },
                RelativeCards: new List<string>() { }
                );

            return config;
        }
    }


    [EntityLogic(typeof(JabReimuCUltDef))]
    public sealed class JabReimuCUlt : UltimateSkill
    {
        public JabReimuCUlt()
        {
            TargetType = TargetType.SingleEnemy;
            GunName = "博丽一拳";
        }

        // remove accurate
        public override DamageInfo Damage => DamageInfo.Attack(Config.Damage);


        protected override IEnumerable<BattleAction> Actions(UnitSelector selector)
        {
            yield return new DamageAction(Owner, selector.GetEnemy(Battle), Damage, GunName, GunType.Single);
        }

    }


}
