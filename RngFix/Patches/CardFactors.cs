using HarmonyLib;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches
{

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BaseCardWeight))]
    class KillCardFactors_Patch
    {

        static bool IgnoreFactorTable() => !BepinexPlugin.ignoreFactorsTableConf.Value;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameRunController), nameof(GameRunController._cardRewardWeightFactors))))
                .MatchForward(false, new CodeMatch(OpCodes.Brfalse))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Pop))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(KillCardFactors_Patch), nameof(KillCardFactors_Patch.IgnoreFactorTable))))
                .InstructionEnumeration();
        }
    }


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetShopCards))]
    class GetShopCards_Patch
    {
        static int IgnoreFactorTable() => BepinexPlugin.ignoreFactorsTableConf.Value ? 0 : 1;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            return new CodeMatcher(instructions)
                .SearchForward((ci) => ci.operand is MethodBase mb && mb.Name == "RollCards")
                .MatchBack(false, new CodeMatch[] { OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_0 })
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetShopCards_Patch), nameof(GetShopCards_Patch.IgnoreFactorTable))))
                .InstructionEnumeration();
        }
    }




}
