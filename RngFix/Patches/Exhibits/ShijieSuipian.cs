using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.Exhibits.Shining;
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
    class FragmentsOfTheWorld_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(ShijieSuipian), nameof(ShijieSuipian.SpecialGain));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.GameRunEventRng), AccessTools.Method(typeof(ExhibitSelfRngs), nameof(ExhibitSelfRngs.GetSelfRng), new Type[] { typeof(GameRunController), typeof(Exhibit) }))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                //
                .ReplaceRngGetter(nameof(GameRunController.GameRunEventRng), AccessTools.Method(typeof(ExhibitSelfRngs), nameof(ExhibitSelfRngs.GetSelfRng), new Type[] { typeof(GameRunController), typeof(Exhibit) }))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .InstructionEnumeration();
        }
    }


}
