using HarmonyLib;
using LBoL.Core;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches.Exhibits
{

    [HarmonyPatch]
    class LightBulb_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(GapOptionsPanel), nameof(GapOptionsPanel.GetRareCardRunner));
        }


        static string LightBulbId() => nameof(ShanliangDengpao);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.AdventureRng), AccessTools.Method(typeof(ExhibitSelfRngs), nameof(ExhibitSelfRngs.GetSelfRng), new Type[] { typeof(GameRunController), typeof(string) }))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LightBulb_Patch), nameof(LightBulb_Patch.LightBulbId))))
                .InstructionEnumeration();
        }

    }
}
