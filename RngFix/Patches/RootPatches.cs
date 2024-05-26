using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
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
            grRngs.rootBattleRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.rootStationRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.AssignStationRngs(() => new RandomGen(grRngs.rootStationRng.NextULong()));

        }

        // custom rngs are initialized right after root rng is created, ensuring any additional rootRng calls in future updates won't influence custom rngs seeding.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
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
        static void Prefix(GameRunController __instance, Station station)
        {
            var grRngs = GrRngs.GetOrCreate(__instance);
            var st = station.Type;
            if ((st is StationType.Entry && (station.Level != 0 || station.Act != 1))
                || st is StationType.Select
                || st is StationType.Supply
                || st is StationType.Trade)
            {
                log.LogDebug($"station type: {st}");

                grRngs.AssignStationRngs(() => new RandomGen(grRngs.rootStationRng.NextULong()));
            }

            var battleRng = grRngs.rootBattleRng;
            GrRngs.AssignNodeRngs(__instance, () => new RandomGen(battleRng.NextULong()));

        }
    }


}
