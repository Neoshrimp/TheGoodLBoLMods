using LBoL.Core;
using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace FullElite.BattleModifiers.Args
{
    public class ArbitraryBattleEventArgs : GameEventArgs
    {
        public Unit Target { get; set; }
        public ModMod unitAction { get; set; }

        public override string GetBaseDebugString()
        {
            return $"ArbitraryAction: {DebugString(Target)}";
        }
    }
}
