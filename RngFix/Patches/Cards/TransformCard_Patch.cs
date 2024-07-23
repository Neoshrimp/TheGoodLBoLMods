using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


namespace RngFix.Patches.Cards
{
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollTransformCard))]
    class TransformCard_Patch
    {
        static RandomGen GetSubRng(RandomGen rng) => new RandomGen(rng.NextULong());

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var subRng_local = generator.DeclareLocal(typeof(RandomGen));
            return new CodeMatcher(instructions)
                .MatchEndForward(new CodeMatch(OpCodes.Ldarg_1))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TransformCard_Patch), nameof(TransformCard_Patch.GetSubRng))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_S, subRng_local.LocalIndex))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_S, subRng_local.LocalIndex))


                .MatchEndForward(new CodeMatch(OpCodes.Ldarg_1))
                .SetInstruction(new CodeInstruction(OpCodes.Ldloc_S, subRng_local.LocalIndex))

                .InstructionEnumeration();
        }

    }
}
