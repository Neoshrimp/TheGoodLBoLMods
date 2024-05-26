using LBoL.Base;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RngFix.CustomRngs
{
    public class GrRngs
    {
        static ConditionalWeakTable<GameRunController, GrRngs> table = new ConditionalWeakTable<GameRunController, GrRngs>();


        public RandomGen rootBattleRng;
        public RandomGen rootStationRng;

        public RandomGen battleLootRng;


        //2do needs to be saved
        public RandomGen enemyStationRng;
        public static RandomGen GetEnemyStationRng(GameRunController gr) => GetOrCreate(gr).enemyStationRng;
        public RandomGen eliteStationRng;
        public static RandomGen GetEliteStationRng(GameRunController gr) => GetOrCreate(gr).eliteStationRng;
        public RandomGen eventStationRng;
        public static RandomGen GetEventStationRng(GameRunController gr) => GetOrCreate(gr).eventStationRng;



        public void AssignStationRngs(Func<RandomGen> rngProvider)
        {
            enemyStationRng = rngProvider();
            eliteStationRng = rngProvider();
            eventStationRng = rngProvider();
        }   

        public static void AssignNodeRngs(GameRunController gr, Func<RandomGen> rngProvider)
        {
            gr.GameRunEventRng = rngProvider(); // this still leaves some manipulation possible but w/e
            gr.BattleRng = rngProvider();
            gr.BattleCardRng = rngProvider();
            gr.ShuffleRng = rngProvider();
            gr.EnemyMoveRng = rngProvider();
            gr.EnemyBattleRng = rngProvider();

            GetOrCreate(gr).battleLootRng = rngProvider();
        }

        static public GrRngs GetOrCreate(GameRunController gr)
        {
            var grRgns = table.GetOrCreateValue(gr);
            return grRgns;
        }
    }
}
