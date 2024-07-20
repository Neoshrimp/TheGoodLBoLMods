using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Units;
using RngFix.CustomRngs;
using System.Collections.Generic;
using System.Reflection.Emit;
using static RngFix.BepinexPlugin;

namespace RngFix.Patches.Battle
{
    // avoids any potential patches
    [HarmonyPatch]
    public class OGEnemyBattleRng
    {
        [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.EnemyBattleRng), MethodType.Getter)]
        [HarmonyReversePatch]
        static public RandomGen EnemyBattleRng(GameRunController gr) => null;
    }

    [HarmonyPatch(typeof(EnemyUnit), "EnterGameRun")]
    class EnemyUnitEnterGameRun_Patch
    {
        

        static RandomGen GetMaxHpRng(GameRunController gr, Unit unit)
        {
            if(unit.Battle != null) // rng is assigned by Nodemaster and hp roll order shouldn't matter
                return UnitRngs.GetOrCreate(unit, gr, unit.Battle).maxHpRng;

            return OGEnemyBattleRng.EnemyBattleRng(gr);
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instruction)
        {
            return new CodeMatcher(instruction)
                .ReplaceRngGetter(nameof(GameRunController.EnemyBattleRng), AccessTools.Method(typeof(EnemyUnitEnterGameRun_Patch), nameof(EnemyUnitEnterGameRun_Patch.GetMaxHpRng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .InstructionEnumeration();
        }

    }



    [HarmonyPatch(typeof(Unit), nameof(Unit.EnterBattle))]
    class InitUnitRng_Patch
    {
        static void Prefix(Unit __instance, BattleController battle)
        {
            UnitRngs.GetOrCreate(__instance, __instance.GameRun, battle);
        }
    }








    [HarmonyPatch(typeof(EnemyUnit), nameof(EnemyUnit.EnemyMoveRng), MethodType.Getter)]
    class EnemyMoveRng_Patch
    {

        static RandomGen GetEnemyMove1Rng(GameRunController gr, EnemyUnit unit)
        {
            var rez = UnitRngs.GetOrCreate(unit, gr, unit.Battle).moveRng1;
            log.LogDebug($"move1rng {unit.GetType().Name} {unit.RootIndex} {rez.State}");
            return rez;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.EnemyMoveRng), AccessTools.Method(typeof(EnemyMoveRng_Patch), nameof(EnemyMoveRng_Patch.GetEnemyMove1Rng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .InstructionEnumeration();
        }

    }




}

