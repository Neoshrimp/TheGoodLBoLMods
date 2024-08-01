using LBoL.ConfigData;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Adventure;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoLEntitySideloader;
using LBoLEntitySideloader.Attributes;
using LBoLEntitySideloader.Resource;
using System;
using System.Collections.Generic;
using System.Text;
using VariantsC.Shared;

namespace VariantsC.Marisa.C
{
    public sealed class FakePackForCommonLocDef : RootRandomStartersDef
    {
        public override IdContainer GetId() => nameof(FakePackForCommonLoc);

        public override CardImages LoadCardImages()
        {
            return null;
        }
    }


    [EntityLogic(typeof(FakePackForCommonLocDef))]
    public sealed class FakePackForCommonLoc : Card
    { }


    public sealed class BasicTreasurePackDef : RootRandomStartersDef
    {
        public override IdContainer GetId() => nameof(BasicTreasurePack);

        public override CardConfig MakeConfig()
        {
            var con = base.MakeConfig();
            con.Illustrator = "lif";
            return con;
        }
    }

    [EntityLogic(typeof(BasicTreasurePackDef))]
    public sealed class BasicTreasurePack : RootRandomStarter
    {
        public override int AddTimes => 1;

        public override RandomPoolEntry<Type[]>[] PotentialCardTypes => new RandomPoolEntry<Type[]>[] {
            new RandomPoolEntry<Type[]>(new Type[] { typeof(JungleMaster) }, 1),
            //new RandomPoolEntry<Type[]>(new Type[] { typeof(Potion) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Astrology), typeof(Astrology) }, 1f),
        };
    }


    public sealed class BasicAttacksPackDef : RootRandomStartersDef
    {
        public override IdContainer GetId() => nameof(BasicAttacksPack);

        public override CardConfig MakeConfig()
        {
            var con = base.MakeConfig();
            con.Illustrator = "鬼针草";
            return con;
        }
    }

    [EntityLogic(typeof(BasicAttacksPackDef))]
    public sealed class BasicAttacksPack : RootRandomStarter
    {
        public override int AddTimes => 5;

        public override RandomPoolEntry<Type[]>[] PotentialCardTypes => new RandomPoolEntry<Type[]>[] {
            new RandomPoolEntry<Type[]>(new Type[] { typeof(MarisaAttackB) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(MarisaAttackR) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Shoot) }, 1.2f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Shoot), typeof(Shoot) }, 0.5f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(BalancedBasicCard) }, 0.8f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(MarisaAttackB), typeof(MarisaAttackR), typeof(BalancedBasicCard) }, 0.2f),

            
        };
    }


    public sealed class BasicDefencePackDef : RootRandomStartersDef
    {
        public override IdContainer GetId() => nameof(BasicDefencePack);

        public override CardConfig MakeConfig()
        {
            var con = base.MakeConfig();
            con.Illustrator = "@kyotyanehh";
            return con;
        }
    }

    [EntityLogic(typeof(BasicDefencePackDef))]
    public sealed class BasicDefencePack : RootRandomStarter
    {
        public override int AddTimes => 5;

        public override RandomPoolEntry<Type[]>[] PotentialCardTypes => new RandomPoolEntry<Type[]>[] {
            new RandomPoolEntry<Type[]>(new Type[] { typeof(MarisaBlockB) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(MarisaBlockR) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Boundary) }, 1f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(BalancedBasicCard) }, 0.8f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Boundary), typeof(BalancedBasicCard) }, 0.35f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Boundary), typeof(Boundary), typeof(Boundary) }, 0.2f),
        };
    }


    public sealed class BasicMyseryPackDef : RootRandomStartersDef
    {
        public override IdContainer GetId() => nameof(BasicMiseryPack);

        public override CardConfig MakeConfig()
        {
            var con = base.MakeConfig();
            con.Illustrator = "ル一キ一ドリフト";
            return con;
        }
    }

    [EntityLogic(typeof(BasicMyseryPackDef))]
    public sealed class BasicMiseryPack : RootRandomStarter
    {
        public override int AddTimes => 3;

        public override RandomPoolEntry<Type[]>[] PotentialCardTypes => new RandomPoolEntry<Type[]>[] {
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Yueguang) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Riguang) }, 1),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Shadow) }, 1f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(BlackResidue) }, 0.8f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(WolfFur) }, 0.75f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Xingguang), typeof(HatateNews) }, 0.8f),
            new RandomPoolEntry<Type[]>(new Type[] { typeof(Xingguang), typeof(AyaNews) }, 0.75f),
        };
    }
}
