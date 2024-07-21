using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.SaveData;
using LBoL.Core.Units;
using RngFix.CustomRngs;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static RngFix.BepinexPlugin;

namespace RngFix.Patches.Battle
{


    [HarmonyPatch]
    class EnemyRngDistributor
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.EnemyBattleRng));
            yield return AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.EnemyMoveRng));
        }


        static void Postfix(GameRunController __instance, ref RandomGen __result, MethodBase __originalMethod)
        {

            var battle = __instance.Battle;
            if (battle == null)
                return;

            var brngs = BattleRngs.GetOrCreate(battle);
            var grrngs = GrRngs.GetOrCreate(__instance);


            var caller = OnDemandRngs.FindCallingEntity();
            string callerName = caller.FullName;

            if (caller.IsSubclassOf(typeof(Unit)) || caller == typeof(Unit))
            {
                log.LogWarning($"Using lost unit rng for {callerName}");
                callerName = OnDemandRngs.GetId(callerName);
                if (brngs.lostUnitRngs.TryGetValue(callerName, out var unitRngs))
                {
                    unitRngs = new UnitRngs(brngs.independentEnemyRngs.GetSubRng(callerName, grrngs.NodeMaster.rng.State));
                    brngs.lostUnitRngs.Add(callerName, unitRngs);
                }

                var getterName = __originalMethod.Name[4..];
                switch (getterName)
                {
                    case nameof(GameRunController.EnemyMoveRng):
                        __result = unitRngs.moveRng1;
                        break;
                    case nameof(GameRunController.EnemyBattleRng):
                        __result = unitRngs.battleRng1;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                __result = brngs.independentEnemyRngs.GetSubRng(callerName, grrngs.NodeMaster.rng.State);
                //log.LogDebug("independentEnmeyrng " + callerName + " " + __result.State.ToString());
            }


        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }

    }





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
            //log.LogDebug($"move1rng {unit.GetType().Name} {unit.RootIndex} {rez.State}");
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


    [HarmonyPatch(typeof(EnemyUnit), nameof(EnemyUnit.EnemyBattleRng), MethodType.Getter)]
    class EnemyBattleRng_Patch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .ReplaceRngGetter(nameof(GameRunController.EnemyBattleRng), AccessTools.Method(typeof(GetUnitRng), nameof(GetUnitRng.BattleOneRng)))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                .InstructionEnumeration();
        }

    }



}

