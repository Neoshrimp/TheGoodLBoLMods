using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using RngFix.CustomRngs;
using System.Collections.Generic;
using System.Reflection;

namespace RngFix.Patches.Battle
{


    [HarmonyPatch]
    class RngDistributor
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.BattleRng));
            yield return AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.BattleCardRng));
        }


        static void Postfix(GameRunController __instance, ref RandomGen __result, MethodBase __originalMethod)
        {

            var battle = __instance.Battle;
            if (battle == null)
                return;

            string callerName = null;

            if (EntityToCallchain.TryConsume(RandomAliveEnemy_AttachRngId_Patch.GetAttachId(), out var entity))
            {
                callerName  = entity.GetType().FullName;
            }


            var brngs = BattleRngs.GetOrCreate(battle);
            var grrngs = GrRngs.GetOrCreate(__instance);
            if(callerName == null)
                callerName = OnDemandRngs.FindCallingEntity().FullName;



            // sometimes Sub is correct

            var getterName = __originalMethod.Name[4..];

            switch (getterName)
            {
                case nameof(GameRunController.BattleRng):
                    __result = brngs.battleRngs.GetSubRng(callerName, grrngs.NodeMaster.rng.State);
                    break;
                case nameof(GameRunController.BattleCardRng):
                    __result = brngs.battleCardRngs.GetSubRng(callerName, grrngs.NodeMaster.rng.State);
                    break;
                default:
                    break;
            }



        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }

    }


}
