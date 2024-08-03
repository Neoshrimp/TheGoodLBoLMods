using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace RngFix.Patches.Battle
{
    [HarmonyPatch]
    public class CallFriends_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(CallFriends), nameof(CallFriends.Actions));
        }


        static RandomGen FakeRng(GameRunController gr) => new RandomGen();

        static void CheckShuffle(List<Type> rez, RandomGen fakeRng, CallFriends callFriends)
        {
            if (rez.Count <= 1)
                return;

            var allTypes = callFriends._types;
            var subRng = callFriends.GameRun.BattleRng;
            allTypes.Shuffle(subRng);

            var toAdd = allTypes.First(t => rez.Contains(t));
            rez.Clear();
            rez.Add(toAdd);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("Shuffle"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CallFriends_Patch), nameof(CallFriends_Patch.CheckShuffle))))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))

                .SearchBack(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("Where"))

                .ReplaceRngGetter(nameof(GameRunController.BattleRng), AccessTools.Method(typeof(CallFriends_Patch), nameof(CallFriends_Patch.FakeRng)))

                .LeaveJumpFix().InstructionEnumeration();
        }

    }

}
