using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.EntityLib.Adventures.Stage3;
using LBoL.Presentation.UI;
using LBoL.Presentation.UI.Panels;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace RngFix.Patches.Events
{
    public class DancersRng
    {
        public RandomGen optionRng;
        public RandomGen toolRng;

        public RandomGen exhibitRng;
        public RandomGen abilityRng;

    }

    [HarmonyPatch(typeof(BackgroundDancers))]
    class DancersInit_Patch
    {
        static ConditionalWeakTable<BackgroundDancers, DancersRng> table = new ConditionalWeakTable<BackgroundDancers, DancersRng>();

        public static DancersRng GerOrCreateRng(Adventure adventure)
        {
            if (adventure is BackgroundDancers bd)
                return table.GetOrCreateValue(bd);
            return null;
        }

        public static RandomGen GetOptionRng(BackgroundDancers bd) => GerOrCreateRng(bd).optionRng;
        public static RandomGen GetToolRng(BackgroundDancers bd) => GerOrCreateRng(bd).toolRng;
        public static RandomGen GetExhibitRng(BackgroundDancers bd) => GerOrCreateRng(bd).exhibitRng;
        public static RandomGen GetAbilityRng(BackgroundDancers bd) => GerOrCreateRng(bd).abilityRng;



        [HarmonyPatch(nameof(BackgroundDancers.InitVariables))]
        [HarmonyPrefix]
        static void InitPrefix(BackgroundDancers __instance)
        {
            var dr = GerOrCreateRng(__instance);
            var gr = __instance.GameRun;

            dr.optionRng = new RandomGen(gr.AdventureRng.NextULong());
            dr.toolRng = new RandomGen(gr.AdventureRng.NextULong());
            dr.exhibitRng = new RandomGen(gr.AdventureRng.NextULong());
            dr.abilityRng = new RandomGen(gr.AdventureRng.NextULong());
        }



    }



    [HarmonyPatch]
    class DancersRollSelect_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BackgroundDancers), nameof(BackgroundDancers.RollOptions));
            yield return ExtraAccess.InnerMoveNext(typeof(BackgroundDancers), nameof(BackgroundDancers.SelectOption));
        }



        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
        {
            var thisOpCode = __originalMethod.Name == "MoveNext" ? OpCodes.Ldloc_1 : OpCodes.Ldarg_0;


            return new CodeMatcher(instructions)
                // options
                .ReplaceRngGetter(nameof(GameRunController.AdventureRng), AccessTools.Method(typeof(DancersInit_Patch), nameof(DancersInit_Patch.GetOptionRng)))
                .Insert(new CodeInstruction(thisOpCode))
                .Insert(new CodeInstruction(OpCodes.Pop))
                // tools
                .ReplaceRngGetter(nameof(GameRunController.AdventureRng), AccessTools.Method(typeof(DancersInit_Patch), nameof(DancersInit_Patch.GetToolRng)))
                .Insert(new CodeInstruction(thisOpCode))
                .Insert(new CodeInstruction(OpCodes.Pop))
                // abilities
                .ReplaceRngGetter(nameof(GameRunController.AdventureRng), AccessTools.Method(typeof(DancersInit_Patch), nameof(DancersInit_Patch.GetAbilityRng)))
                .Insert(new CodeInstruction(thisOpCode))
                .Insert(new CodeInstruction(OpCodes.Pop))
                .LeaveJumpFix().InstructionEnumeration();
        }

    }


    [HarmonyPatch(typeof(Stage), nameof(Stage.GetSpecialAdventureExhibit))]
    class GetSpecialAdventureExhibit_Patch
    {

        static RandomGen GetRng(GameRunController gr)
        {
            try
            {
                var vnPanel = UiManager.GetPanel<VnPanel>();
                var adventure = vnPanel._currentAdventure;
                var exRng = DancersInit_Patch.GerOrCreateRng(adventure)?.exhibitRng;
                if (exRng != null)
                    return exRng;

            }
            catch (InvalidOperationException)
            {
            }

            return gr.ExhibitRng;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.ExhibitRng), AccessTools.Method(typeof(GetSpecialAdventureExhibit_Patch), nameof(GetSpecialAdventureExhibit_Patch.GetRng)))
                 .LeaveJumpFix().InstructionEnumeration();
        }
    }


}
