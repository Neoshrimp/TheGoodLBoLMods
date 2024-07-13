using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Common;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static RngFix.BepinexPlugin;


namespace RngFix.Patches.Battle
{


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BattleRng), MethodType.Getter)]
    class BattleRng_Patch
    {
        static void Prefix(GameRunController __instance)
        {
            var caller = Helpers.FindCallingEntity();
            log.LogDebug(caller.Name);

        }



        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }

    }


}
