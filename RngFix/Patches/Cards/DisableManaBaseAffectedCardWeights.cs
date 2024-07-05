using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using System.Collections.Generic;
using System.Reflection.Emit;


namespace RngFix.Patches.Cards
{
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BaseCardWeight))]
    class DisableManaBaseAffectedCardWeights_Patch
    {
        public static bool tempDebugDisable = false;

        static int CheckOption(int colourCount)
        {
            if (BepinexPlugin.disableManaBaseAffectedCardWeights.Value || tempDebugDisable)
                return 0;
            return colourCount;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions)
                .End()
                .MatchEndBackwards(new CodeMatch[]
                {
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CardConfig), nameof(CardConfig.Colors))),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(IReadOnlyCollection<ManaColor>), nameof(IReadOnlyCollection<ManaColor>.Count))),
                    OpCodes.Stloc_S,
                    OpCodes.Ldloc_S
                })
                .Advance(-1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DisableManaBaseAffectedCardWeights_Patch), nameof(DisableManaBaseAffectedCardWeights_Patch.CheckOption))))
                .InstructionEnumeration();
        }


    }


}
