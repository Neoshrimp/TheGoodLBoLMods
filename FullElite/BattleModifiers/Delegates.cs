using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace FullElite.BattleModifiers
{
    public delegate bool ModPrecond(Unit unit);
    public delegate Unit ModMod(Unit unit);
}
