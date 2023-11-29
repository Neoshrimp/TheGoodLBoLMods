using FullElite.BattleModifiers.Args;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace FullElite.BattleModifiers.Actions
{
    public class ArbitraryBattleAction : SimpleEventBattleAction<ArbitraryBattleEventArgs>
    {

        public ArbitraryBattleAction(Unit unit, ModMod unitAction) 
        {
            Args = new ArbitraryBattleEventArgs();
            Args.Target = unit;
            Args.unitAction = unitAction;
        }


        public override void MainPhase()
        {
            try
            {
                Args.unitAction(Args.Target);
            }
            catch (Exception ex )
            {
                BepinexPlugin.log.LogError($"Exception during {this.GetType().Name}: {ex}");
            }
        }
    }
}
