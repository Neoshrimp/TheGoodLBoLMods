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


            __result = brngs.battleRngs.GetSubRng(caller.FullName, grrngs.NodeMaster.rng.State);

            
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions;
        }

    }


    [HarmonyPatch]
    //[HarmonyDebug]
    class LockRandomTurnManaAction_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(LockRandomTurnManaAction), nameof(LockRandomTurnManaAction.ResolvePhaseEnumerator));
            //yield return AccessTools.Method(typeof(AddCardsToDrawZoneAction), nameof(AddCardsToDrawZoneAction.MainPhase));
            //yield return ExtraAccess.InnerMoveNext(typeof(MoveCardToDrawZoneAction), nameof(MoveCardToDrawZoneAction.GetPhases));

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
            var battle = gr.Battle;
            //var overdraftSubRng = new RandomGen(grrngs.overdraftRng.NextULong());

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


            var matcher = new CodeMatcher(instructions);
            while (true)
            {
                try
                {
                    matcher = matcher.ReplaceRngGetter(nameof(GameRunController.BattleRng), AccessTools.Method(typeof(LockRandomTurnManaAction_Patch), nameof(LockRandomTurnManaAction_Patch.GetEntityRng)));
                    matcher = matcher
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1));
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
