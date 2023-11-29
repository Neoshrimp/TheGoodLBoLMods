﻿using LBoL.Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FullElite.BattleModifiers
{
    public class UnitModifier
    {
        public string unitId;
        public bool modifyPlayer = false;

        public UnitModifier(string groupId, bool modifyPlayer = false)
        {
            this.unitId = groupId;
            UnitModTracker.Instance.AddModifier(this);
            this.modifyPlayer = modifyPlayer;
        }

        public HashSet<ModPrecond> preconds = new HashSet<ModPrecond>();
        public List<ModMod> mods = new List<ModMod>();

        public void ApplyMods(Unit unit)
        {
            if(unit is PlayerUnit && !modifyPlayer)
                return; 
            if (preconds.Count > 0 && preconds.Select(p => p(unit)).Any(b => !b))
                return;
            foreach (var m in mods)
            {
                try
                {
                    m.Invoke(unit);
                }
                catch (Exception ex)
                {
                    BepinexPlugin.log.LogError(ex);
                }
            }
        }
    }
}
