﻿using HarmonyLib;
using LBoL.Core.Units;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.Stages.NormalStages;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace FullElite
{


    [HarmonyPatch]
    class Doremy_Hp_Patch
    {

        public class MulStore 
        { 
            public float mul = 1f;
            public static bool SetHpGainMul(Doremy doremy, float mul)
            {
                try
                {
                    var mulStore = mulTable.GetOrCreateValue(doremy);
                    mulStore.mul = mul;
                }
                catch (Exception)
                {
                    BepinexPlugin.log.LogWarning($"Could not set hp multiplier for: {doremy}");
                    return false;
                }
                return true;
                
            }
        }
        public static ConditionalWeakTable<Doremy, MulStore> mulTable = new ConditionalWeakTable<Doremy, MulStore>();



        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(Doremy), nameof(Doremy.SleepActions));
        }

        static int ApplyMul(int target, Doremy doremy)
        {

            var rez = target;
            if(mulTable.TryGetValue(doremy, out var mulStore))
            {
                rez = (int)Math.Round(target * mulStore.mul);
            }
            return rez;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                // hp
                .MatchForward(true, new CodeMatch[] {
                    new CodeMatch(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Unit), nameof(Unit.Hp)))),
                    new CodeMatch(OpCodes.Ldc_I4_S)
                })
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Doremy_Hp_Patch), nameof(Doremy_Hp_Patch.ApplyMul))))
                // maxHp
                .MatchForward(true, new CodeMatch[] {
                    new CodeMatch(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Unit), nameof(Unit.MaxHp)))),
                    new CodeMatch(OpCodes.Ldc_I4_S)
                })
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Doremy_Hp_Patch), nameof(Doremy_Hp_Patch.ApplyMul))))

                .InstructionEnumeration();   
        }


    }



    [HarmonyPatch(typeof(Kokoro), nameof(Kokoro.GetTurnMoves), MethodType.Enumerator)]
    class Kokoro_GlareCount_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .SearchForward(ci => ci.opcode == OpCodes.Call && (ci?.operand.ToString().Contains("AddCardMove") ?? false))
                .MatchStartBackwards(new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(EnemyUnit), nameof(EnemyUnit.Difficulty))))
                .ThrowIfInvalid("Difficulty check not matched")
                .MatchStartForward(OpCodes.Ldc_I4_2)
                .SetAndAdvance(OpCodes.Ldloc_1, null)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Kokoro_GlareCount_Patch), nameof(Kokoro_GlareCount_Patch.CheckStage))))
                .InstructionEnumeration();
        }

        private static int CheckStage(Kokoro kokoro)
        {
            if (kokoro.GameRun?.CurrentStage?.GetType() == typeof(BambooForest))
            { 
                return 1;
            }
            return 2;
        }

    }




}
