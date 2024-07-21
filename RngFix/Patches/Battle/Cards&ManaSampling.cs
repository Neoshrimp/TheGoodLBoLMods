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
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Cards.Character.Marisa;
using LBoL.EntityLib.Cards.Character.Reimu;
using LBoL.EntityLib.Cards.Character.Sakuya;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.EntityLib.Cards.Neutral.Black;
using LBoL.EntityLib.Cards.Neutral.NoColor;
using LBoL.EntityLib.Cards.Neutral.TwoColor;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.Exhibits.Shining;
using LBoL.EntityLib.JadeBoxes;
using LBoL.EntityLib.StatusEffects.Cirno;
using LBoL.EntityLib.StatusEffects.Enemy;
using LBoL.EntityLib.StatusEffects.Enemy.SeijaItems;
using LBoL.EntityLib.StatusEffects.Marisa;
using LBoL.EntityLib.StatusEffects.Neutral.TwoColor;
using LBoL.EntityLib.StatusEffects.Neutral.White;
using LBoL.EntityLib.StatusEffects.Sakuya;
using LBoL.Presentation.UI.Widgets;
using LBoLEntitySideloader;
using LBoLEntitySideloader.CustomHandlers;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using RngFix.CustomRngs.Sampling.Pads;
using System;
using System.Collections;
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

    //technically hand targeting could much more sophisticated, by taking into what in the hand and selecting rng state accordingly but w/e

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

            var remainingList = remainingManaBase.ToList();
            var rez = new List<ManaColor>();

            for (int o = 0; o < count; o++)
            {
                if (remainingList.Count == 0)
                    break;
                var samplingRng = new RandomGen(seedCarrier.State);
                var overdraftOrder = Padding.PadManaColours(remainingList, inverse: true, groupSize: 12);

                overdraftOrder.Shuffle(samplingRng);
                var colour = overdraftOrder.First(c => c != null);
                rez.Add(colour.Value);
                remainingList.Remove(colour.Value);

                //log.LogDebug(string.Join(";", overdraftOrder.Where(c => c != null)));
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

            return brngs.battleRngs.GetOrCreateRootRng(actionSourceId, grrngs.NodeMaster.rng.State);
        }

        static int ConsistentDeckPos(RandomGen subRng, int _zero, int deckCount)
        {
            if (deckCount == 0)
                return 0;
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
            cwt_actionUsers.AddOrUpdate(card, action?.Source?.GetType().FullName ?? card.GetType().FullName);
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
            Attach_InsertActionSource_Patch.cwt_actionUsers.AddOrUpdate(card, action?.Source?.GetType().FullName ?? card.GetType().FullName);


        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions)
                .MatchEndForward(OpCodes.Ldfld);
            var instance = matcher.Instruction;


            return matcher
                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CardMovingToDrawZoneEventArgs), nameof(CardMovingToDrawZoneEventArgs.Card))))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(instance)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Attach_MoveActionSource_Patch), nameof(Attach_MoveActionSource_Patch.AttachSource))))

                .InstructionEnumeration();
        }


    }



    // used to consume attached property
    [HarmonyPatch(typeof(BattleController), nameof(BattleController.RandomAliveEnemy), MethodType.Getter)]
    class RandomAliveEnemy_Patch
    {

        static bool Prefix(BattleController __instance, ref EnemyUnit __result)
        {
            var b = __instance;
            if (b.EnemyGroup.Alives.Count() <= 1)
            {
                EntityToCallchain.TryConsume(RandomAliveEnemy_AttachRngId_Patch.GetAttachId(), out var _);
                __result = b.EnemyGroup.Alives.FirstOrDefault();
                return false;
            }

            return true;
        }


    }



    [HarmonyPatch(typeof(Card), nameof(Card.AttackAction), new Type[] { typeof(UnitSelector), typeof(DamageInfo), typeof(GunPair) })]
    class RandomAliveEnemy_AttachRngId_Patch
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

                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RandomAliveEnemy_AttachRngId_Patch), nameof(RandomAliveEnemy_AttachRngId_Patch.AttachCard))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))


                .InstructionEnumeration();
        }

    }


    [HarmonyPatch]
    class RandomHandAction_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Card), nameof(Card.UpgradeRandomHandAction));
            yield return AccessTools.Method(typeof(Card), nameof(Card.DiscardRandomHandAction));

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
                    var rng = BattleRngs.GetOrCreate(__instance.Battle).battleRngs.GetSubRng(__instance.GetType().FullName, GrRngs.GetOrCreate(__instance.GameRun).NodeMaster.rng.State);

                    var randomizedHand = BattleRngs.Shuffle(rng, hand.ToList());
                    __result = new DiscardManyAction(randomizedHand);
                    return false;
                }

            return true;
        }


    }



    [HarmonyPatch]
    class TargetSingleEnemy_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(BattleController), nameof(BattleController.RandomAliveEnemy));
            yield return ExtraAccess.InnerMoveNext(typeof(IceLaser), nameof(IceLaser.Actions));
            yield return ExtraAccess.InnerMoveNext(typeof(Changzhizhen), nameof(Changzhizhen.OnPlayerTurnEnding));
            yield return ExtraAccess.InnerMoveNext(typeof(SakuyaAttackX), nameof(SakuyaAttackX.Actions));
            // enemyRng
            yield return ExtraAccess.InnerMoveNext(typeof(HetongKailang), nameof(HetongKailang.RepairActions));
            yield return ExtraAccess.InnerMoveNext(typeof(YinyangyuBlueOrigin), nameof(YinyangyuBlueOrigin.DefendActions));


        }

        static EnemyUnit SampleByRootIndex(IEnumerable<EnemyUnit> alives, RandomGen subRng)
        {

            var aliveDic = alives.ToDictionary(e => e.RootIndex, e => e);

            if (aliveDic.Count == 0)
                return null;

            if (aliveDic.Count == 1)
                return alives.First();

            int max = Math.Max(20, aliveDic.Count);
            var allPos = new List<int>();
            for (int i = 0; i < max; i++)
                allPos.Add(i);
            allPos.Shuffle(subRng);
            var rez = aliveDic[allPos.First(i => aliveDic.ContainsKey(i))];
            return rez;
        }



        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
        {

            CodeMatch searchBackMatch = null;

            var declaringEntityName = OnDemandRngs.FindDeclaringGameEntity(__originalMethod.DeclaringType)?.Name ?? "";
            switch (declaringEntityName)
            {
                case nameof(IceLaser):
                    searchBackMatch = new CodeMatch(ci => ci.opcode == OpCodes.Call && (ci.operand?.ToString().Contains("Where") ?? false));
                    break;
                case nameof(SakuyaAttackX):
                    searchBackMatch = new CodeMatch(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(BattleController), nameof(BattleController.AllAliveEnemies))));
                    break;
                case nameof(HetongKailang):
                    searchBackMatch = new CodeMatch(OpCodes.Ldloc_2);
                    break;
                case nameof(YinyangyuBlueOrigin):
                    searchBackMatch = new CodeMatch(OpCodes.Ldloc_2);
                    break;
                default:
                    break;
            }

            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && (mb.Name.StartsWith("SampleOrDefault") || mb.Name == "Sample"))

                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TargetSingleEnemy_Patch), nameof(TargetSingleEnemy_Patch.SampleByRootIndex))))
                .RngAdvancementGuard(generator, searchBackMatch)

                .InstructionEnumeration();
        }


    }

    [HarmonyPatch]
    class TargetMultipleEnemies_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(Potion), nameof(Potion.EnterHandReactor));
            yield return ExtraAccess.InnerMoveNext(typeof(ForeverCoolSe), nameof(ForeverCoolSe.OnCardUsed));


        }

        static EnemyUnit[] SampleByRootIndex(IEnumerable<EnemyUnit> alives, int amount, RandomGen subRng)
        {

            var aliveDic = alives.ToDictionary(e => e.RootIndex, e => e);

            if (aliveDic.Count == 0 || amount <= 0)
                return new EnemyUnit[0];

            if (amount >= aliveDic.Count)
                return alives.ToArray();

            int max = Math.Max(20, aliveDic.Count);
            var allPos = new List<int>();
            for (int i = 0; i < max; i++)
                allPos.Add(i);
            allPos.Shuffle(subRng);

            var rez = new EnemyUnit[amount];
            int index = 0;
            foreach (var p in allPos)
            {
                if (aliveDic.TryGetValue(p, out var enemy))
                {
                    rez[index] = enemy;
                    index++;
                    if (index >= amount)
                        break;
                    aliveDic.Remove(p);
                }
            }

            return rez;
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
        {

            CodeMatch searchBackMatch = new CodeMatch(ci =>
                        (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt)
                            && (ci.operand is MethodInfo mi
                            && mi.ReturnType.GetInterfaces().FirstOrDefault(i => i == typeof(IEnumerable)) != null
                        ));


            CodeMatch[] amountMatch = new CodeMatch[] {
                new CodeMatch(ci => ci.IsLdloc() || ci.opcode == OpCodes.Ldarg_0),
                new CodeMatch(ci => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && (ci.operand is MethodBase mb && mb.Name.Contains("Level")))
            };
            var declaringEntityName = OnDemandRngs.FindDeclaringGameEntity(__originalMethod.DeclaringType)?.Name ?? "";

            switch (declaringEntityName)
            {
                case nameof(Potion):
                    amountMatch = null;
                    break;
                default:
                    break;
            }


            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && (mb.Name.StartsWith("SampleManyOrAll") || mb.Name == "SampleMany"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TargetMultipleEnemies_Patch), nameof(TargetMultipleEnemies_Patch.SampleByRootIndex))))

                .RngAdvancementGuard(generator, searchBackMatch, many: true, amountMatch: amountMatch)

                .InstructionEnumeration();
        }
    }



    [HarmonyPatch]
    class TargetSingleCard_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(MoonPurify), "Actions");
            yield return ExtraAccess.InnerMoveNext(typeof(MeilingWater), "Actions");
            yield return ExtraAccess.InnerMoveNext(typeof(SadinYuebing), nameof(SadinYuebing.OnPlayerTurnStarted));
            yield return ExtraAccess.InnerMoveNext(typeof(TangSan), nameof(TangSan.OnPlayerTurnStarted));
            yield return ExtraAccess.InnerMoveNext(typeof(YouxiangWakeSe), nameof(YouxiangWakeSe.OnPlayerTurnStarted));
            yield return AccessTools.Method(typeof(MoonWorldSe), nameof(MoonWorldSe.OnTurnStarted));


        }

        static Card SampleCard(IEnumerable<Card> pool, RandomGen rng)
        {
            if (pool.Count() == 0)
                return null;
            if (pool.Count() == 1)
                return pool.First();
            var subRng = rng;
            var randomizedHand = BattleRngs.Shuffle(subRng, pool.ToList());
            return randomizedHand.First();

        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
        {

            CodeMatch[] searchBackMatches = new CodeMatch[] { new CodeMatch(ci =>
                        (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt)
                            && (ci.operand is MethodInfo mi
                            && mi.ReturnType.GetInterfaces().FirstOrDefault(i => i == typeof(IEnumerable)) != null
                        )) };



            var declaringEntityName = OnDemandRngs.FindDeclaringGameEntity(__originalMethod.DeclaringType)?.Name ?? "";

            switch (declaringEntityName)
            {
                case nameof(MoonPurify):
                    searchBackMatches = new CodeMatch[] { OpCodes.Ldloc_3, OpCodes.Ldloc_S };
                    break;
                case nameof(MeilingWater):
                case nameof(TangSan):
                    searchBackMatches = new CodeMatch[] { OpCodes.Ldloc_2 };
                    break;
                case nameof(SadinYuebing):
                    searchBackMatches = new CodeMatch[] { OpCodes.Ldloc_3, OpCodes.Ldloc_2 };
                    break;
                case nameof(YouxiangWakeSe):
                    searchBackMatches = new CodeMatch[] { OpCodes.Ldloc_S };
                    break;
                case nameof(MoonWorldSe):
                    searchBackMatches = new CodeMatch[] { OpCodes.Ldloc_2, OpCodes.Ldloc_3 };
                    break;
                default:
                    break;
            }

            var matcher = new CodeMatcher(instructions);
            int i = 0;
            while (true)
            {
                try
                {
                    matcher = matcher
                        .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && (mb.Name.StartsWith("SampleOrDefault") || mb.Name == "Sample"))
                        .ThrowIfInvalid("")
                        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TargetSingleCard_Patch), nameof(TargetSingleCard_Patch.SampleCard))));
                    if (declaringEntityName != nameof(Card))
                    {
                        matcher.RngAdvancementGuard(generator, searchBackMatches[Math.Min(i, searchBackMatches.Length - 1)],
                        many: false);
                    }
                    i++;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            return matcher.InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    class TargetMultipleCards_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Card), nameof(Card.UpgradeRandomHandAction));
            yield return AccessTools.Method(typeof(Card), nameof(Card.DiscardRandomHandAction));
            yield return ExtraAccess.InnerMoveNext(typeof(IceWing), nameof(IceWing.Actions));
            yield return ExtraAccess.InnerMoveNext(typeof(FangxiangHeal), "Actions");
            yield return ExtraAccess.InnerMoveNext(typeof(SatoriMemory), "Actions");
            yield return AccessTools.Method(typeof(Bingzhilin), nameof(Bingzhilin.OnCardUsed));
            yield return ExtraAccess.InnerMoveNext(typeof(PiaoliangPanzi), nameof(PiaoliangPanzi.OnPlayerTurnStarted));
            yield return ExtraAccess.InnerMoveNext(typeof(ForeverCoolSe), nameof(ForeverCoolSe.OnCardUsed));
            yield return ExtraAccess.InnerMoveNext(typeof(YonglinUpgradeSe), nameof(YonglinUpgradeSe.OnPlayerTurnStarted));

            yield return ExtraAccess.InnerMoveNext(typeof(SuwakoLimao), nameof(SuwakoLimao.HexActions));
            yield return ExtraAccess.InnerMoveNext(typeof(InfinityGemsSe), nameof(InfinityGemsSe.OnOwnerTurnStarted));
            yield return ExtraAccess.InnerMoveNext(typeof(SuwakoHex), nameof(SuwakoHex.OnPlayerTurnStarted));


        }

        static Card[] SampleCards(IEnumerable<Card> pool, int amount, RandomGen rng)
        {
            if (amount >= pool.Count() || amount <= 0)
            {
                return pool.ToArray();
            }
            var subRng = rng;
            var randomizedHand = BattleRngs.Shuffle(subRng, pool.ToList());
            return randomizedHand.GetRange(0, amount).ToArray();

        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase __originalMethod)
        {

            CodeMatch searchBackMatch = new CodeMatch(ci =>
                        (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt)
                        && (ci.operand is MethodInfo mi
                        && mi.ReturnType.GetInterfaces().FirstOrDefault(i => i == typeof(IEnumerable)) != null
                        ));


            CodeMatch[] amountMatch = new CodeMatch[] {
                new CodeMatch(ci => ci.IsLdloc() || ci.opcode == OpCodes.Ldarg_0),
                new CodeMatch(ci => (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && (ci.operand is MethodBase mb && (mb.Name.Contains("Value") || mb.Name.Contains("Level"))))
            };


            var declaringEntityName = OnDemandRngs.FindDeclaringGameEntity(__originalMethod.DeclaringType)?.Name ?? "";
            var searchForAmountMatchFirst = false;

            switch (declaringEntityName)
            {
                case nameof(IceWing):
                    searchBackMatch = new CodeMatch(new CodeInstruction(OpCodes.Ldloc_3));
                    break;
                case nameof(Bingzhilin):
                    searchBackMatch = new CodeMatch(new CodeInstruction(OpCodes.Ldloc_0));
                    break;
                case nameof(SuwakoLimao):
                    amountMatch = new CodeMatch[] {
                        new CodeMatch(OpCodes.Ldloc_2),
                        new CodeMatch(ci => ci.opcode == OpCodes.Call && (ci.operand is MethodBase mb &&  mb.Name.Contains("Count")))
                    };
                    break;
                case nameof(InfinityGemsSe):
                    searchBackMatch = new CodeMatch(new CodeInstruction(OpCodes.Ldloc_2));

                    amountMatch = new CodeMatch[] {
                        new CodeMatch(OpCodes.Ldloc_2),
                        new CodeMatch(ci => ci.opcode == OpCodes.Callvirt && (ci.operand is MethodBase mb &&  mb.Name.Contains("Count"))),
                        new CodeMatch(OpCodes.Ldc_I4_2),
                        new CodeMatch(OpCodes.Div),
                    };
                    searchForAmountMatchFirst = true;
                    break;
                case nameof(SuwakoHex):
                    searchBackMatch = new CodeMatch(ci => ci.opcode == OpCodes.Ldfld && (ci.operand?.ToString().Contains("cards") ?? false));
                    break;
                default:
                    break;
            }

            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher
                        .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && (mb.Name.StartsWith("SampleManyOrAll") || mb.Name == "SampleMany"))
                        .ThrowIfInvalid("")
                        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(TargetMultipleCards_Patch), nameof(TargetMultipleCards_Patch.SampleCards))));

                    if (declaringEntityName != nameof(Card)) // guard is handled in prefix
                    {
                        matcher.RngAdvancementGuard(generator, searchBackMatch,
                        many: true,
                        amountMatch: amountMatch,
                        searchForAmountMatchFirst: searchForAmountMatchFirst
                        );
                    }
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            return matcher.InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    class SampleSingleMana_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(JingQiXi), nameof(JingQiXi.OnOwnerTurnStarted));

        }


        private static ManaColor SampleSingleMana(IEnumerable<ManaColor> pool, RandomGen subRng)
        {
            if (pool.Count() == 0)
                return default;

            if (pool.Count() == 1)
                return pool.First();

            var samplingPool = Padding.PadManaColours(pool);
            samplingPool.Shuffle(subRng);

            ManaColor rez = default;
            foreach (var c in samplingPool)
            {
                if (c == null)
                    continue;
                rez = c.Value;
                break;
            }
            return rez;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {


            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher
                        .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && (mb.Name.StartsWith("SampleOrDefault") || mb.Name == "Sample"))
                        .ThrowIfInvalid("")
                        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SampleSingleMana_Patch), nameof(SampleSingleMana_Patch.SampleSingleMana))));

                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            return matcher.InstructionEnumeration();


        }


    }



    [HarmonyPatch]
    class SampleMultipleMana_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(ConvertManaAction), nameof(ConvertManaAction.PhilosophyRandomMana));
            yield return ExtraAccess.InnerMoveNext(typeof(Yueguang), nameof(Yueguang.OnDraw));
            yield return AccessTools.Method(typeof(SlowMana), nameof(SlowMana.OnCardUsed));
            yield return AccessTools.Method(typeof(MasterOfCollectionSe), nameof(MasterOfCollectionSe.OnCardRetaining));
            yield return ExtraAccess.InnerMoveNext(typeof(PerfectServantWSe), nameof(PerfectServantWSe.OnOwnerTurnStarted));
            yield return AccessTools.Method(typeof(StopTimeSe), nameof(StopTimeSe.OnManaLosing));


        }


        private static ManaColor[] SampleMana(IEnumerable<ManaColor> pool, int amount, RandomGen subRng)
        {
            var samplingPool = Padding.PadManaColours(pool);
            samplingPool.Shuffle(subRng);

            var rez = new List<ManaColor>();
            int a = 0;
            foreach (var c in samplingPool)
            {
                if (c == null)
                    continue;
                rez.Add(c.Value);
                a++;
                if (a >= amount)
                    break;
            }
            return rez.ToArray();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {


            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher
                        .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && (mb.Name.StartsWith("SampleManyOrAll") || mb.Name == "SampleMany"))
                        .ThrowIfInvalid("")
                        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SampleMultipleMana_Patch), nameof(SampleMultipleMana_Patch.SampleMana))));
                  
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
