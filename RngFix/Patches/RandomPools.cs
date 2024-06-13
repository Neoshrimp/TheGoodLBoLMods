using HarmonyLib;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.Patches
{
    // 2do filter by generic parameter
    //[HarmonyPatch]
    //[HarmonyDebug]
    class NormalizeWeights_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            //yield return AccessTools.Method(typeof(UniqueRandomPool<>), "Sample", new Type[] { typeof(Func<float, float, float>) });


            yield return typeof(UniqueRandomPool<>).MakeGenericType(typeof(Card)).GetMethod("Sample", new Type[] { typeof(Func<float, float, float>) });
        }




        static float DivideItemWeight(float w, float totalW) 
        {
            var rez = w / totalW;
            //log.LogDebug(rez);
            return rez;
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeMatch[] { new CodeInstruction(OpCodes.Ldc_R4, 0f), new CodeInstruction(OpCodes.Ldloc_0) })
                .Set(OpCodes.Ldc_R4, 1f)

                .MatchForward(true, new CodeMatch[] { new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(RandomPoolEntry<Card>), "Weight")) })
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NormalizeWeights_Patch), nameof(NormalizeWeights_Patch.DivideItemWeight))))

                .Advance(1)
                .MatchForward(true, new CodeMatch[] { new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(RandomPoolEntry<Card>), "Weight")) })
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NormalizeWeights_Patch), nameof(NormalizeWeights_Patch.DivideItemWeight))))


                .InstructionEnumeration();
        }
    }
}