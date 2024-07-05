using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
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
using System.Reflection.Emit;

namespace RngFix.Patches.Battle
{

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.ShuffleDrawPile))]
    class ShuffleDrawPile_Patch
    {
        static bool Prefix(BattleController __instance)
        {
            var bc = __instance;

            return false;
        }
    }


    [HarmonyPatch(typeof(BattleController), MethodType.Constructor, new Type[] { typeof(GameRunController), typeof(EnemyGroup), typeof(IEnumerable<Card>) })]
    class BattleController_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions)
                 .InstructionEnumeration();
        }

    }



}
