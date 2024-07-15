using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.Core.Units;
using LBoL.EntityLib.Exhibits.Common;
using LBoLEntitySideloader.CustomHandlers;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using static RngFix.BepinexPlugin;


namespace RngFix.Patches.Battle
{


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BattleRng), MethodType.Getter)]
    class BattleRng_Patch
    {


        static void Postfix(GameRunController __instance, ref RandomGen __result)
        {

            var battle = __instance.Battle;
            if (battle == null)
                return;

            var brngs = BattleRngs.GetOrCreate(battle);
            var grrngs = GrRngs.GetOrCreate(__instance);

            var caller = OnDemandRngs.FindCallingEntity();


            __result = brngs.entityRngs.GetSubRng(caller.FullName, grrngs.NodeMaster.rng.State);

            
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }

    }


    [HarmonyPatch]
    class LockRandomTurnManaAction_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(LockRandomTurnManaAction), nameof(LockRandomTurnManaAction.ResolvePhaseEnumerator));

        }

        public static void RegisterAdvanceOverdraftRngReactor()
        {
            CHandlerManager.RegisterBattleEventHandler(
                b => b.Player.TurnStarting,
                args => {
                    var gr = GrRngs.Gr();
                    var grrngs = GrRngs.GetOrCreate(gr);

                    grrngs.overdraftRng.NextULong();
                },
                null,
                GameEventPriority.Highest
                );
        }

        static RandomGen GetEntityRng(GameRunController gr)
        {

            var grrngs = GrRngs.GetOrCreate(gr);

            return grrngs.overdraftRng;
        }

        static ManaColor[] SampleOverdraft(IEnumerable<ManaColor> remainingManaBase, int count, RandomGen seedCarrier) 
        {
            var gr = GrRngs.Gr();
            var battle = gr.Battle;

            var samplingRng = new RandomGen(seedCarrier.State);

            var overdraftOrder = battle.BaseTurnMana.EnumerateComponents().ToList();

            //2do pad (not really)
            //int pad = 20;

            overdraftOrder.Shuffle(samplingRng);

            var rez = new List<ManaColor>();
            int sampleCount = 0;
            var remainingSet = new HashSet<ManaColor>(remainingManaBase);
            foreach (var c in overdraftOrder)
            {
                if (remainingSet.Contains(c))
                {
                    rez.Add(c);
                    remainingSet.Remove(c);
                    sampleCount++;
                    if (sampleCount >= count)
                        break;

                }
            }

            return rez.ToArray();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            RegisterAdvanceOverdraftRngReactor();
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.BattleRng), AccessTools.Method(typeof(LockRandomTurnManaAction_Patch), nameof(LockRandomTurnManaAction_Patch.GetEntityRng)))
                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("SampleManyOrAll"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LockRandomTurnManaAction_Patch), nameof(LockRandomTurnManaAction_Patch.SampleOverdraft))))
                .InstructionEnumeration();
        }

    }



    [HarmonyPatch]
    class AddCardToDrawZone_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(BattleController), nameof(BattleController.AddCardToDrawZone));
            yield return AccessTools.Method(typeof(BattleController), nameof(BattleController.MoveCardToDrawZone));

        }


        static RandomGen GetEntityRng(GameRunController gr, Card card)
        {
            var battle = gr.Battle;
            var grrngs = GrRngs.GetOrCreate(gr);
            var brngs = BattleRngs.GetOrCreate(battle);

            if (!Attach_InsertActionSource_Patch.cwt_actionUsers.TryGetValue(card, out var actionSourceId))
                actionSourceId = card.GetType().FullName;

            return brngs.entityRngs.GetSubRng(actionSourceId, grrngs.NodeMaster.rng.State);
        }

        static int ConsistentDeckPos(RandomGen subRng, int _zero, int deckCount)
        {
            int max = Math.Max(2000, deckCount);
            var allPos = new List<int>();
            for (int i = 0; i < max; i++)
            {
                allPos.Add(i);
            }
            allPos.Shuffle(subRng);
            var rez = allPos.First(p => p <= deckCount);
            return rez;

        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.BattleRng), AccessTools.Method(typeof(AddCardToDrawZone_Patch), nameof(AddCardToDrawZone_Patch.GetEntityRng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_1))
                .MatchStartForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RandomGen), nameof(RandomGen.NextInt))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddCardToDrawZone_Patch), nameof(AddCardToDrawZone_Patch.ConsistentDeckPos))))

                .InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    class Attach_InsertActionSource_Patch
    {
        public static ConditionalWeakTable<Card, string> cwt_actionUsers = new ConditionalWeakTable<Card, string>();


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(AddCardsToDrawZoneAction), nameof(AddCardsToDrawZoneAction.MainPhase));
        }


        static void AttachSource(Card card, BattleAction action)
        {
            cwt_actionUsers.AddOrUpdate(card, action.Source.GetType().FullName);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(OpCodes.Ldloc_2)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Attach_InsertActionSource_Patch), nameof(Attach_InsertActionSource_Patch.AttachSource))
                ))
                .InstructionEnumeration();
        }


    }

    [HarmonyPatch]
    class Attach_MoveActionSource_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(MoveCardToDrawZoneAction).GetNestedTypes(AccessTools.all).First(t => t.Name.Contains("c__DisplayClass5_0")).GetMethods(AccessTools.all).First(m => m.Name.Contains("GetPhases"));
        }


        static void AttachSource(Card card, BattleAction action)
        {
            Attach_InsertActionSource_Patch.cwt_actionUsers.AddOrUpdate(card, action.Source.GetType().FullName);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions)
                .MatchEndForward(OpCodes.Ldfld);
            var instance = matcher.Instruction;

                
            return matcher
                .MatchEndForward(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CardMovingToDrawZoneEventArgs), nameof(CardMovingToDrawZoneEventArgs.Card))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Attach_MoveActionSource_Patch), nameof(Attach_MoveActionSource_Patch.AttachSource))
                ))
                .Insert(instance)
                .InstructionEnumeration();
        }


    }







}
