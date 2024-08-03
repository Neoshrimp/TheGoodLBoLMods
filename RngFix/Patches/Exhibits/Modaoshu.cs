using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Common;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches.Exhibits
{

    [HarmonyPatch]
    class MagicGuideBook_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(Modaoshu), nameof(Modaoshu.SpecialGain));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.CardRng), AccessTools.Method(typeof(ExhibitsSelfRngs), nameof(ExhibitsSelfRngs.GetSelfRng), new Type[] {typeof(GameRunController), typeof(Exhibit)}))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .LeaveJumpFix().InstructionEnumeration();
        }

    }


}
