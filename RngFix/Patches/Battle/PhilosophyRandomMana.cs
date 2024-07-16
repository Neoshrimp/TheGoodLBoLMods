using HarmonyLib;
using LBoL.Core.Battle.BattleActions;
using System;
using System.Collections.Generic;
using System.Text;

namespace RngFix.Patches.Battle
{
    [HarmonyPatch(typeof(ConvertManaAction), nameof(ConvertManaAction.PhilosophyRandomMana))]
    class PhilosophyRandomMana
    {
    }
}
