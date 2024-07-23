using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.EntityLib.Exhibits;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;


namespace RngFix.Patches.Cards
{
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BaseCardWeight))]
    class FlattenBaseWeights_Patch
    {
        public static bool tempDebugDisable = false;

        static int CheckOption(int colourCount)
        {
            if (tempDebugDisable)
                return 0;
            return colourCount;
        }

        static int NormalizeAmount(ref ManaGroup _, GameRunController gr)
        {
            
            return gr.Player.Config.InitialMana.Amount + gr.Player.Exhibits.Where(ex => ex is ShiningExhibit).Count();
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
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FlattenBaseWeights_Patch), nameof(FlattenBaseWeights_Patch.CheckOption))))
                
                .MatchEndForward(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(ManaGroup), nameof(ManaGroup.Amount))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FlattenBaseWeights_Patch), nameof(FlattenBaseWeights_Patch.NormalizeAmount))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))

                .InstructionEnumeration();
        }


    }


}
