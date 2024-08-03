using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Adventures.Stage2;
using RngFix.CustomRngs;
using RngFix.Patches.Exhibits;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace RngFix.Patches.RngGetters
{


    [HarmonyPatch]
    class RareExhibitRoll_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(MiyoiBartender), nameof(MiyoiBartender.InitVariables));
            yield return AccessTools.Method(typeof(YachieOppression), nameof(YachieOppression.InitVariables));

        }

        public static Exhibit RollExhibitInAdventure(Stage stage, ExhibitWeightTable weightTable, Predicate<ExhibitConfig> filter = null)
        {
            var gr = stage.GameRun;
            return gr.RollNormalExhibit(gr.AdventureRng, weightTable, new Func<Exhibit>(stage.GetSentinelExhibit), filter);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
               .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Stage), nameof(Stage.RollExhibitInAdventure))))
               .Set(OpCodes.Call, AccessTools.Method(typeof(RareExhibitRoll_Patch), nameof(RareExhibitRoll_Patch.RollExhibitInAdventure)))
               .LeaveJumpFix().InstructionEnumeration();
        }


    }




}
