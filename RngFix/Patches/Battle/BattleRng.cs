﻿using HarmonyLib;
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
using LBoL.Presentation.UI.Widgets;
using LBoLEntitySideloader.CustomHandlers;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using static RngFix.BepinexPlugin;


namespace RngFix.Patches.Battle
{

    //2do
    //technically hand targeting could much more sophisticated, by taking into what in the hand and selecting rng state accordingly but w/e


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BattleRng), MethodType.Getter)]
    class BattleRng_Patch
    {


        static void Postfix(GameRunController __instance, ref RandomGen __result)
        {

            var battle = __instance.Battle;
            if (battle == null)
                return;

            string callerName = null;

            if (EntityToCallchain.TryConsume(RandomAliveEnemy_RngId_Patch.GetAttachId(), out var entity))
            {
                callerName  = entity.GetType().FullName;
            }


            if(callerName == null)
                callerName = OnDemandRngs.FindCallingEntity().FullName;

            var brngs = BattleRngs.GetOrCreate(battle);
            var grrngs = GrRngs.GetOrCreate(__instance);
            __result = brngs.entityRngs.GetOrCreateRootRng(callerName, grrngs.NodeMaster.rng.State);

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

            return brngs.entityRngs.GetOrCreateRootRng(actionSourceId, grrngs.NodeMaster.rng.State);
        }

        static int ConsistentDeckPos(RandomGen rng, int _zero, int deckCount)
        {
            if (deckCount == 0)
                return 0;
            var subRng = new RandomGen(rng.NextULong());
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



    [HarmonyPatch(typeof(BattleController), nameof(BattleController.RandomAliveEnemy), MethodType.Getter)]
    class RandomAliveEnemy_Patch
    {

        static EnemyUnit SampleByRootIndex(IEnumerable<EnemyUnit> alives, RandomGen rng)
        {

            var aliveDic = alives.ToDictionary(e => e.RootIndex, e => e);

            // do not advance the rng
            if (aliveDic.Count == 1)
                return alives.First();

            var subRng = new RandomGen(rng.NextULong());

            int max = Math.Max(20, aliveDic.Count);
            var allPos = new List<int>();
            for (int i = 0; i < max; i++)
                allPos.Add(i);
            allPos.Shuffle(subRng);
            var rez = aliveDic[allPos.First(i => aliveDic.ContainsKey(i))];
            return rez;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("SampleOrDefault"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomAliveEnemy_Patch), nameof(RandomAliveEnemy_Patch.SampleByRootIndex))))
                .InstructionEnumeration();
        }
    }



    [HarmonyPatch(typeof(Card), nameof(Card.AttackAction), new Type[] { typeof(UnitSelector), typeof(DamageInfo), typeof(GunPair) })]
    class RandomAliveEnemy_RngId_Patch
    {

        public static string GetAttachId() => "AttackAction";

        static void AttachCard(Card card)
        {
            EntityToCallchain.Attach(GetAttachId(), card);
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(UnitSelector), nameof(UnitSelector.GetEnemy))))
                .Advance(1)
                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(UnitSelector), nameof(UnitSelector.GetEnemy))))

                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomAliveEnemy_RngId_Patch), nameof(RandomAliveEnemy_RngId_Patch.AttachCard))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))


                .InstructionEnumeration();
        }

    }


    [HarmonyPatch]
    //[HarmonyDebug]
    class RandomHandAction_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Card), nameof(Card.UpgradeRandomHandAction));
            yield return AccessTools.Method(typeof(Card), nameof(Card.DiscardRandomHandAction));

        }


        static Card[] SampleCards(IEnumerable<Card> hand, int amount, RandomGen rng, int cutoff)
        {
            if (cutoff > 0 && amount >= hand.Count())
            {
                rng.NextULong();
                return hand.ToArray();
            }
            var subRng = rng;
            var randomizedHand = BattleRngs.Shuffle(subRng, hand.ToList());
            return randomizedHand.GetRange(0, amount).ToArray();

        }

        static bool Prefix(Card __instance, int amount, ref BattleAction __result, MethodBase __originalMethod)
        {
            var hand = __instance.Battle.HandZone;
            if (__originalMethod.Name.StartsWith("Upgrade"))
            {
                var canUpgrade = hand.Where(c => c.CanUpgradeAndPositive).ToList();
                if (amount >= canUpgrade.Count)
                { 
                    __result = new UpgradeCardsAction(canUpgrade);
                    return false;
                }
                return true;
            }
            if (__originalMethod.Name.StartsWith("Discard"))
                if (amount >= hand.Count)
                {
                    // technically discard order matters
                    __result = new DiscardManyAction(SampleCards(hand, amount, new RandomGen(__instance.BattleRng.State), 0));
                    return false;
                }

            return true;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher.SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand.ToString().Contains("SampleManyOrAll"))
                        .ThrowIfInvalid("")
                        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomHandAction_Patch), nameof(RandomHandAction_Patch.SampleCards))))
                        .Insert(new CodeInstruction(OpCodes.Ldc_I4_1))
                        .Advance(1);
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            return matcher.InstructionEnumeration();
        }
    }






}
