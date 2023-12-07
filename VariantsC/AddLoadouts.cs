﻿using Cysharp.Threading.Tasks;
using LBoL.ConfigData;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using VariantsC.Reimu;
using System.Linq;

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
                exhibit: nameof(CounterfeitCoinEx),
                deck: new List<string>() { nameof(ReimuAttackR), nameof(ReimuAttackW), nameof(ReimuBlockR), nameof(ReimuBlockW), nameof(BalancedBasicCard), nameof(RollingPebbleCard) }.Concat(baseCards).ToList(),
                complexity: 2);

        }
    }
}