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

namespace RngFix.Patches.Battle
{


    [HarmonyPatch]
    // adavances rng only once per cast
    class ModuoluoSeasons_Patch
    {

        static ConditionalWeakTable<object, RandomGen> cwt_innerEnum2rng = new ConditionalWeakTable<object, RandomGen>();


        static IEnumerable<MethodBase> TargetMethods()
        {
            
            yield return ExtraAccess.InnerMoveNext(typeof(ModuoluoSeasons), nameof(ModuoluoSeasons.Actions));
        }

        private static RandomGen GetRng(GameRunController gr, object innerEnum, ModuoluoSeasons card)
        {
            if (!cwt_innerEnum2rng.TryGetValue(innerEnum, out var rng))
            {
                rng = new RandomGen(card.GameRun.BattleCardRng.NextULong());
                cwt_innerEnum2rng.Add(innerEnum, rng);
            }
            return rng;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            return new CodeMatcher(instructions)

                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.BattleCardRng))))
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModuoluoSeasons_Patch), nameof(ModuoluoSeasons_Patch.GetRng))))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .Insert(new CodeInstruction(OpCodes.Ldarg_0))


                .InstructionEnumeration();
        }


    }


}
