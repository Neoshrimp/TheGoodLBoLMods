using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.FirstPlace;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.Presentation;
using RngFix.CustomRngs;
using RngFix.CustomRngs.Sampling.Pads;
using RngFix.Patches.Debug;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            log.LogDebug("Initing GrRngs..");
            grRngs.persRngs.stageMasterRng = new StageMasterRng(gr.RootRng.NextULong());
            grRngs.persRngs.actMasterRng = new ActMasterRng(gr.RootRng.NextULong());

            grRngs.persRngs.gapInitRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.persRngs.shopInitRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.persRngs.rareExhibitQueueRng = new RandomGen(gr.RootRng.NextULong());
            grRngs.persRngs.qingeUpgradeQueueRng = new RandomGen(gr.RootRng.NextULong());

            grRngs.persRngs.fallbackInitRng = new RandomGen(gr.RootRng.NextULong());

            grRngs.ExhibitSelfRngs = new EntitySelfRngs<Exhibit>(ex => ex.Id, gr.RootRng.NextULong());
            grRngs.ExhibitSelfRngs.InitialiseRngs(Padding.AllExhibits.Select(ec => ec?.Name));

            var cardRngInit = new RandomGen(gr.RootRng.NextULong());

            grRngs.persRngs.cardUpgradeQueueRng = new RandomGen(cardRngInit.NextULong());
            grRngs.persRngs.eliteCardRng = new RandomGen(cardRngInit.NextULong());
            grRngs.persRngs.bossCardRng = new RandomGen(cardRngInit.NextULong());

            grRngs.persRngs.adventureSelfRngs = new EntitySelfRngs<Adventure>(a => a.Id, gr.RootRng.NextULong());
            grRngs.persRngs.adventureSelfRngs.InitialiseRngs(Padding.AllAdventurePadding().Select(ac => ac?.Name));
            
            grRngs.persRngs.shopRngs = ShopRngs.Init(gr.RootRng.NextULong());

            grRngs.persRngs.extraCardRewardRng = new RandomGen(gr.RootRng.NextULong());

            grRngs.unusedRoot0 = new RandomGen(gr.RootRng.NextULong());
            
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
        static void Prefix(GameRunController __instance, Station station) // station is not fully initialized 
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
                    grRngs.NodeMaster = null; // initialized once adventure type is known
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
            else if(node.StationType != StationType.Adventure)
            {
                log.LogError($"NodeRngs were not assigned for station {node.StationType}");
            }


        }

        static void Postfix(GameRunController __instance)
        {
            var gr = __instance;
            StatsLogger.LogGeneral(gr);
            StatsLogger.LogVanillaRngs(gr);
            StatsLogger.LogPersistentRngs(gr);
        }
    }


    [HarmonyPatch(typeof(Stage), nameof(Stage.GetAdventure))]
    class InitAdventureNode_Patch
    {
        static void Postfix(Stage __instance, Type __result)
        {
            var stage = __instance;
            var gr = stage.GameRun;
            var grngs = GrRngs.GetOrCreate(gr);

            ulong seed = grngs.persRngs.adventureSelfRngs.GetRng(__result.Name).NextULong();

            grngs.NodeMaster = new NodeMasterRng(seed);
            grngs.NodeMaster.Advance(gr);
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
