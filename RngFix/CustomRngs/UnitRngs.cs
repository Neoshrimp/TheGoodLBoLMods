using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.Cards.Neutral.White;
using LBoL.Presentation;
using RngFix.CustomRngs.Sampling;
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.CustomRngs.Sampling.UniformPools;
using RngFix.Patches;
using Spine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static RngFix.BepinexPlugin;

namespace RngFix.CustomRngs
{
    public class UnitRngs
    {
        private static ConditionalWeakTable<Unit, UnitRngs> table = new ConditionalWeakTable<Unit, UnitRngs>();

        public static UnitRngs GetOrCreate(Unit unit, GameRunController gr = null, BattleController battle = null)
        {
            if (gr == null)
                gr = GrRngs.Gr();
            if(battle == null)
                battle = BattleRngs.Battle();

            if (battle == null)
                throw new InvalidOperationException("GameRun.Battle is unassigned");

            return table.GetValue(unit, unit => {
                
                var unitRngs = new UnitRngs();
                var initState = GrRngs.GetOrCreate(gr).NodeMaster.rng.State;


                var subRng = BattleRngs.GetOrCreate(battle).unitRootRngs.GetSubRng(unit.GetType().FullName, initState);
                //log.LogDebug($"rng init {unit.Name} {subRng.State}");
                unitRngs.InitRng(subRng);
                return unitRngs;
            });
        }

        public UnitRngs() { }

        public UnitRngs(RandomGen rng) :this() 
        {
            this.InitRng(rng);
        }


        private void InitRng(RandomGen rng)
        {
            this.initialState = rng.State;
            this.maxHpRng = new RandomGen(rng.NextULong());

            this.moveRng1 = new RandomGen(rng.NextULong());
            this.moveRng2 = new RandomGen(rng.NextULong());
            this.moveRng3 = new RandomGen(rng.NextULong());
            this.moveRng4 = new RandomGen(rng.NextULong());
            this.moveRng5 = new RandomGen(rng.NextULong());

            this.battleRng1 = new RandomGen(rng.NextULong());
            this.battleRng2 = new RandomGen(rng.NextULong());
            this.battleRng3 = new RandomGen(rng.NextULong());
            this.battleRng4 = new RandomGen(rng.NextULong());
            this.battleRng5 = new RandomGen(rng.NextULong());

            this.dropRng = new RandomGen(rng.NextULong());


            this.unused0 = new RandomGen(rng.NextULong());
            this.unused1 = new RandomGen(rng.NextULong());
            this.unused2 = new RandomGen(rng.NextULong());
            this.unused3 = new RandomGen(rng.NextULong());
            this.unused4 = new RandomGen(rng.NextULong());
        }

        public ulong initialState;

        public RandomGen maxHpRng;

        public RandomGen moveRng1;
        public RandomGen moveRng2;
        public RandomGen moveRng3;
        public RandomGen moveRng4;
        public RandomGen moveRng5;


        public RandomGen battleRng1;
        public RandomGen battleRng2;
        public RandomGen battleRng3;
        public RandomGen battleRng4;
        public RandomGen battleRng5;


        public RandomGen dropRng;


        public RandomGen unused0;
        public RandomGen unused1;
        public RandomGen unused2;
        public RandomGen unused3;
        public RandomGen unused4;


    }



}
