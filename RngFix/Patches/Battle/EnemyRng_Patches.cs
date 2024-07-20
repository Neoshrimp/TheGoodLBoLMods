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
            log.LogDebug($"move2rng {unit.GetType().Name}");
            return UnitRngs.GetOrCreate(unit, gr).moveRng2;
        }

        public static RandomGen MoveThreeRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr).moveRng3;
        }

        public static RandomGen BattleOneRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr).battleRng1;
        }

        public static RandomGen BattleTwoRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr).battleRng2;
        }

        public static RandomGen BattleThreeRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr).battleRng3;
        }

        public static RandomGen DropRng(GameRunController gr, Unit unit)
        {
            return UnitRngs.GetOrCreate(unit, gr).dropRng;
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
                .InstructionEnumeration();
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


              .InstructionEnumeration();
        }


    }

}
