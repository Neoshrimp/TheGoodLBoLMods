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
        


        public static void AssignBattleRngs(GameRunController gr, Func<RandomGen> rngProvider)
        {
            // blue point and P loot
            // 2do bleeds through and is generally wacko af
            // gr.GameRunEventRng = rngProvider();
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
