using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Exhibits.Shining;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static RngFix.BepinexPlugin;


namespace RngFix.Patches
{
    [HarmonyPatch]
    class Gr_cctor_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Constructor(typeof(GameRunController), new Type[] { typeof(GameRunStartupParameters)});
        }

        static void InitRngs(GameRunController gr)
        {
            var grRngs = GrRngs.GetOrCreate(gr);
            grRngs.rootNodeRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.rootActRng = new RandomGen(gr.RootRng.NextULong());
            GrRngs.AssignActRngs(gr ,() => new RandomGen(grRngs.rootActRng.NextULong()));

        }

        // custom rngs are initialized right after root rng is created, ensuring any additional rootRng calls in future updates won't influence custom rngs seeding.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(GameRunController), nameof(GameRunController.RootRng))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Gr_cctor_Patch), nameof(Gr_cctor_Patch.InitRngs))))
                .InstructionEnumeration();
        }

    }


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterStation))]
    class GameRunController_EnterStation_Patch
    {
        static void Prefix(GameRunController __instance)
        {
            var grRngs = GrRngs.GetOrCreate(__instance);
            var node = __instance.CurrentMap.VisitingNode;
            if (Helpers.IsActTransition(node))
            {
                log.LogDebug("advancing act rng");

                GrRngs.AssignActRngs(__instance, () => new RandomGen(grRngs.rootActRng.NextULong()));
            }
            log.LogDebug("advancing node rng");

            var nodeRng = grRngs.rootNodeRng;
            GrRngs.AssignNodeRngs(__instance, () => new RandomGen(nodeRng.NextULong()));

        }
    }



    [HarmonyPatch(typeof(DoremyPortal.Overrider), nameof(DoremyPortal.Overrider.OnEnteredWithMode))]
    class DoremyPortal_Patch
    {
        static void Prefix(DoremyPortal.Overrider __instance)
        {
            log.LogDebug("Doremy deeznuts");
            var gr = __instance._gameRun;
            GrRngs.AdvanceRngsOnJump(gr, gr.CurrentMap.BossNode);
        }
    }




}
