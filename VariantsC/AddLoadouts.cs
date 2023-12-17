using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using VariantsC.Shared;
using static VariantsC.Reimu.C.ChocolateCoinExDef;
using VariantsC.Reimu.C;
using VariantsC.Sakuya.C;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.UltimateSkills;

namespace VariantsC
{
    public static class CustomLoadouts
    {
        public static void AddLoadouts()
        {
            var baseCards = new List<string>() { nameof(Shoot), nameof(Shoot), nameof(Boundary), nameof(Boundary) };

            
            PlayerUnitTemplate.AddLoadout(
                charId: CharNames.Reimu,
                ultimateSkill: nameof(JabReimuCUlt),
                exhibit: nameof(ChocolateCoinEx),
                deck: new List<string>() { nameof(ReimuAttackR), nameof(ReimuAttackW), nameof(ReimuBlockR), nameof(ReimuBlockW), nameof(BalancedBasicCard), nameof(RollingPebbleCard) }.Concat(baseCards).ToList(),
                complexity: 2);




            PlayerUnitTemplate.AddLoadout(
                charId: CharNames.Sakuya,
                ultimateSkill: nameof(SakuyaUltW),
                exhibit: nameof(BloodyRipperEx),
                deck: new List<string>() {  }.Concat(baseCards).ToList(),
                complexity: 3);

        }
    }
}
