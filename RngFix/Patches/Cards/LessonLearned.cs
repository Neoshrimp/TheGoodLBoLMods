using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RngFix.Patches.Cards
{

    [HarmonyPatch]
    class QingeUpgrade_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(QingeUpgrade), nameof(QingeUpgrade.OnEnemyDied));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.GameRunEventRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetQingeUpgradeQueueRng)))
                 .LeaveJumpFix().InstructionEnumeration();
        }
    }


}
