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
using System.Reflection;
using System.Reflection.Emit;

namespace RngFix.Patches.Battle
{

    [HarmonyPatch(typeof(BattleController), nameof(BattleController.ShuffleDrawPile))]
    class ShuffleDrawPile_Patch
    {
        static bool Prefix(BattleController __instance)
        {
            var bc = __instance;
            var gr = bc.GameRun;
            var shuffledDrawZone = BattleRngs.Shuffle(gr.ShuffleRng, bc._drawZone);
            bc._drawZone.Clear();
            bc._drawZone.AddRange(shuffledDrawZone);

            return false;
        }
    }


    [HarmonyPatch(typeof(BattleController), MethodType.Constructor, new Type[] { typeof(GameRunController), typeof(EnemyGroup), typeof(IEnumerable<Card>) })]
    //[HarmonyDebug]
    class BattleController_ccTor_Patch
    {

        public const int cardDicoveryIdStart = 10000;

        public static int CardDicoveryIdStart => cardDicoveryIdStart;

        static List<Card> ProperShuffle(List<Card> toShuffle, RandomGen rng)
        {
            return BattleRngs.Shuffle(rng, toShuffle);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            

            return new CodeMatcher(instructions)
                 .MatchStartForward(new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(BattleController), nameof(BattleController._cardInstanceId))))
                 .Advance(-1)
                 .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(BattleController_ccTor_Patch), nameof(BattleController_ccTor_Patch.CardDicoveryIdStart))))

                 .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo mi && mi.Name == "Shuffle")
                 .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BattleController_ccTor_Patch), nameof(BattleController_ccTor_Patch.ProperShuffle))))
                 .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_0))


                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodInfo mi && mi.Name == "Shuffle")
                 .SetInstructionAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BattleController_ccTor_Patch), nameof(BattleController_ccTor_Patch.ProperShuffle))))
                 .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_1))

                 .LeaveJumpFix().InstructionEnumeration();
        }

    }



}
