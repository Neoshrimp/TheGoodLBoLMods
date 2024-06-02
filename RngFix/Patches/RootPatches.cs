using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.Presentation;
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
            grRngs.persRngs.stageMasterRng = new StageMasterRng(gr.RootRng.NextULong());
            grRngs.persRngs.actMasterRng = new ActMasterRng(gr.RootRng.NextULong());

            grRngs.persRngs.gapInitRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.persRngs.shopInitRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.persRngs.rareExhibitQueueRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.persRngs.upgradeQueueRng = new RandomGen(gr.RootRng.NextULong());

            grRngs.persRngs.fallbackInitRng = new RandomGen(gr.RootRng.NextULong());

            grRngs.ExhibitSelfRngs = new ExhibitSelfRngs(gr.RootRng.NextULong());
            grRngs.ExhibitSelfRngs.InitialiseExRngs();

            grRngs.unusedRoot0 = new RandomGen(gr.RootRng.NextULong());
            grRngs.unusedRoot1 = new RandomGen(gr.RootRng.NextULong());
            grRngs.unusedRoot2 = new RandomGen(gr.RootRng.NextULong());
            grRngs.unusedRoot3 = new RandomGen(gr.RootRng.NextULong());
            grRngs.unusedRoot4 = new RandomGen(gr.RootRng.NextULong());

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


    // alternative way to 'enter' node after reloading
    [HarmonyPatch(typeof(GameMaster), nameof(GameMaster.BattleStationFlowFromEndSave))]
    class ReEnterAfterBattle_Patch
    {
        static void Prefix(GameMaster __instance)
        {
            //log.LogDebug("reentering..");
            

            var gr = __instance.CurrentGameRun;
            var grRngs = GrRngs.GetOrCreate(gr);

            if (grRngs.NodeMaster == null)
            {

                grRngs.NodeMaster = new NodeMasterRng
                {
                    rng = RandomGen.FromState(grRngs.persRngs.prevNodeMasterState)
                };
                grRngs.NodeMaster.Advance(gr);
            }

            
        }
    }



    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnterStation))]
    class EnterStation_Patch
    {
        static void Prefix(GameRunController __instance)
        {
            //log.LogDebug("seeding..");
            var gr = __instance;
            var grRngs = GrRngs.GetOrCreate(gr);
            var node = gr.CurrentMap.VisitingNode;

            RandomGen nodeInitRng = null;

            switch (node.StationType)
            {
                case StationType.None:
                    nodeInitRng = grRngs.persRngs.fallbackInitRng;
                    log.LogWarning($"Node({node.X}, {node.Y}) station type is {StationType.None} using fallback rng.");
                    break;
                case StationType.Enemy:
                    nodeInitRng = grRngs.persRngs.battleInitRng;
                    break;
                case StationType.EliteEnemy:
                    nodeInitRng = grRngs.persRngs.eliteInitRng;
                    break;
                case StationType.Supply:
                    grRngs.persRngs.actMasterRng.Advance(gr);
                    nodeInitRng = grRngs.persRngs.transitionInitRng;
                    break;
                case StationType.Gap:
                    nodeInitRng = grRngs.persRngs.gapInitRng;
                    break;
                case StationType.Shop:
                    nodeInitRng = grRngs.persRngs.shopInitRng;
                    break;
                case StationType.Adventure:
                    nodeInitRng = grRngs.persRngs.adventureInitRng;
                    break;
                case StationType.Entry:
                    grRngs.persRngs.stageMasterRng.Advance(gr);
                    grRngs.persRngs.actMasterRng.Advance(gr);
                    nodeInitRng = grRngs.persRngs.transitionInitRng;
                    break;
                case StationType.Select:
                    grRngs.persRngs.actMasterRng.Advance(gr);
                    nodeInitRng = grRngs.persRngs.transitionInitRng;
                    break;
                case StationType.Trade:
                    grRngs.persRngs.actMasterRng.Advance(gr);
                    nodeInitRng = grRngs.persRngs.transitionInitRng;
                    break;
                case StationType.Boss:
                    nodeInitRng = grRngs.persRngs.bossInitRng;
                    break;
                case StationType.BattleAdvTest:
                    nodeInitRng = grRngs.persRngs.fallbackInitRng;
                    break;
                default:
                    break;
            }

            if (nodeInitRng != null)
            {
                grRngs.NodeMaster = new NodeMasterRng(nodeInitRng.NextULong());
                grRngs.NodeMaster.Advance(gr);

            }
            else
            {
                log.LogError($"NodeRngs were not assigned for station {node.StationType}");
            }
        }
    }



    [HarmonyPatch(typeof(DoremyPortal.Overrider), nameof(DoremyPortal.Overrider.OnEnteredWithMode))]
    class DoremyPortal_Patch
    {
        static void Prefix(DoremyPortal.Overrider __instance)
        {
            var gr = __instance._gameRun;
            GrRngs.AdvanceRngsOnJump(gr, gr.CurrentMap.BossNode);
        }
    }




}
