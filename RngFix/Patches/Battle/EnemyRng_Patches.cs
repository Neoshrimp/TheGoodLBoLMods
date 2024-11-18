using HarmonyLib;
using System.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Text;
using LBoL.Core;
using System.Linq;
using LBoL.Base;
using RngFix.CustomRngs.Sampling.Pads;
using LBoL.Base.Extensions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Neutral.MultiColor;
using System.Reflection;
using LBoLEntitySideloader.ReflectionHelpers;
using System.Runtime.CompilerServices;
using RngFix.CustomRngs;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Normal.Bats;
using LBoL.Core.Randoms;
using static RngFix.BepinexPlugin;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Ravens;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.Core.Battle;
using UnityEngine;

namespace RngFix.Patches.Battle
{
    public static class GetUnitRng
    {
/*        public static RandomGen MoveOneRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr).moveRng1;
        }*/

        public static RandomGen MoveTwoRng(GameRunController gr, Unit unit)
        {
            //log.LogDebug($"move2rng {unit.GetType().Name}");
            return UnitRngs.GetOrCreate(unit, gr, unit.Battle).moveRng2;
        }

        public static RandomGen MoveThreeRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr, unit.Battle).moveRng3;
        }

        // reserved for main rng getter
        public static RandomGen BattleOneRng(GameRunController gr, Unit unit)
        {
            var rez =  UnitRngs.GetOrCreate(unit, gr, unit.Battle).battleRng1;
            //log.LogDebug($"battle1Rng {unit.GetType().Name} {rez.State}");
            return rez;
        }

        public static RandomGen BattleTwoRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr, unit.Battle).battleRng2;
        }

        public static RandomGen BattleThreeRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr, unit.Battle).battleRng3;
        }

        public static RandomGen DropRng(GameRunController gr, Unit unit, BattleController battle)
        {
            return UnitRngs.GetOrCreate(unit, gr, battle).dropRng;
        }

        public static T ConsistentPoolSample<T>(RepeatableRandomPool<T> pool, RandomGen rng, HashSet<T> withOut)
        {
            var subRng = new RandomGen(rng.NextULong());
            T rez = default;
            int maxAttempts = (int)1E6;
            int i = 0;
            while (true)
            {
                var s = pool.Sample(subRng);
                if (!withOut.Contains(s))
                {
                    rez = s;
                    break;
                }
                if (i >= maxAttempts)
                {
                    log.LogWarning($"No result while consistently sampling the pool.");
                    break;
                }
                i++;
            }
            return rez;
        }
    }


    [HarmonyPatch(typeof(BattleController), nameof(BattleController.GenerateEnemyPoints))]
    class GenerateEnemyPoints_Patch
    {
        static RandomGen GetLootRng(GameRunController gr, BattleController battle, Unit unit) => GetUnitRng.DropRng(gr, unit, battle);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
               .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.GameRunEventRng))))
               .Set(OpCodes.Call, AccessTools.Method(typeof(GenerateEnemyPoints_Patch), nameof(GenerateEnemyPoints_Patch.GetLootRng)))
               .Insert(new CodeInstruction(OpCodes.Ldarg_1))
               .Insert(new CodeInstruction(OpCodes.Ldarg_0))
               .LeaveJumpFix().InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    class Kokoro_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(Kokoro), nameof(Kokoro.XiActions));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter("", AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.MoveTwoRng)), AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.EnemyMoveRng)))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .LeaveJumpFix().InstructionEnumeration();
        }

    }

    [HarmonyPatch(typeof(BatOrigin), nameof(BatOrigin.UpdateMoveCounters))]
    class Bat_Patch
    {

        static bool RollForLockOn(IEnumerable<BatOrigin> bats, Func<BatOrigin, bool> _, BatOrigin bat)
        {
            int maxRoot = bats.Max(b => b.RootIndex);
            if (bat.RootIndex < maxRoot)
                return false;

            int enemyCount = bat.AllAliveEnemies.Count();
            var rolls = new float[enemyCount];
            for (int i = 0; i < Math.Max(enemyCount, 20); i++)
            {
                if(i < enemyCount)
                    rolls[i] = BattleRngs.GetOrCreate(bat.Battle).BatLockOnRng.NextFloat(0f, 1f) * (enemyCount-1);
            }
            return rolls.All(f => f > 0.4f);
        }
        private static BatOrigin.MoveType SampleMove(RepeatableRandomPool<BatOrigin.MoveType> _, RandomGen rng, BatOrigin bat)
        {
            return GetUnitRng.ConsistentPoolSample(new RepeatableRandomPool<BatOrigin.MoveType>(bat._pool), rng, new HashSet<BatOrigin.MoveType>(new BatOrigin.MoveType[] { bat.Last }));
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.opcode == OpCodes.Call && ci.operand is MethodBase mb && mb.Name.StartsWith("All"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Bat_Patch), nameof(Bat_Patch.RollForLockOn))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))


                .SearchForward(ci => ci.opcode == OpCodes.Callvirt && ci.operand.ToString().Contains("Sample"))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Bat_Patch), nameof(Bat_Patch.SampleMove))))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))


              .LeaveJumpFix().InstructionEnumeration();
        }

    }


    [HarmonyPatch]
    class Arg0_Move2_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Guihuo), nameof(Guihuo.SetFirstTurn));
            yield return AccessTools.Method(typeof(MaoyuOrigin), nameof(MaoyuOrigin.SetFirstTurn));

            yield return AccessTools.Method(typeof(Marisa), nameof(Marisa.UpdateMoveCounters));
            yield return AccessTools.Method(typeof(Reimu), nameof(Reimu.UpdateMoveCounters));

            // Reimu defend countdown is static


        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter("", AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.MoveTwoRng)), AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.EnemyMoveRng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))

                
                .LeaveJumpFix().InstructionEnumeration();
        }
    }

    [HarmonyPatch]
    class OnEnterBattle_Move2_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Raven), nameof(Raven.OnEnterBattle));
            // kinda irrelevant?
            yield return AccessTools.Method(typeof(YinyangyuBlueOrigin), nameof(YinyangyuBlueOrigin.OnEnterBattle));
            yield return AccessTools.Method(typeof(YinyangyuRedOrigin), nameof(YinyangyuRedOrigin.OnEnterBattle));


        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter("", AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.MoveTwoRng)), AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.EnemyMoveRng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .LeaveJumpFix().InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    class RavenNormalStart_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Raven), nameof(Raven.NormalStart));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter("", AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.MoveThreeRng)), AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.EnemyMoveRng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .LeaveJumpFix().InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    [HarmonyPriority(Priority.LowerThanNormal)]
    class YinttangBlue_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(YinyangyuBlueOrigin), nameof(YinyangyuBlueOrigin.DefendActions));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter("", AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.MoveThreeRng)), AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.EnemyMoveRng)))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .LeaveJumpFix().InstructionEnumeration();
        }

    }

    [HarmonyPatch]
    class MaskOrigin_Battle2_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(MaskOrigin), nameof(MaskOrigin.Debuff));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter("", AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.BattleTwoRng)), AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.EnemyBattleRng)))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .LeaveJumpFix().InstructionEnumeration();
        }
    }



  




}
                            