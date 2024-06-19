using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Stations;
using LBoL.EntityLib.Adventures.Stage2;
using LBoL.EntityLib.Exhibits.Adventure;
using LBoL.EntityLib.Exhibits.Shining;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;

namespace RngFix.Patches
{
    public static class GenerateTools
    {
        public static Card[] GetTools(Stage stage, int count, RandomGen rng)
        {
            var gr = stage.GameRun;

            Card[] array = gr.RollCards(rng, stage.ShopToolCardWeight, count, new ManaGroup?(gr.BaseMana), gr.RewardAndShopCardColorLimitFlag == 0, false, false, false, null);
            foreach (Card card in array)
            {
                gr.UpgradeNewDeckCardOnFlags(card);
            }
            return array;
        }
    }



    [HarmonyPatch(typeof(RingoEmp), nameof(RingoEmp.InitVariables))]
    class RingoEmp_Patch
    {

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Stage), nameof(Stage.GetShopToolCards))))
                .Set(OpCodes.Call, AccessTools.Method(typeof(GenerateTools), nameof(GenerateTools.GetTools)))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.AdventureRng))))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.Gr))))
                .InstructionEnumeration();
        }

    }

    [HarmonyPatch]
    class FixBook_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(FixBook), nameof(FixBook.SpecialGain));
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Stage), nameof(Stage.GetShopToolCards))))
                .Set(OpCodes.Call, AccessTools.Method(typeof(GenerateTools), nameof(GenerateTools.GetTools)))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExhibitSelfRngs), nameof(ExhibitSelfRngs.GetSelfRng), new Type[] { typeof(GameRunController), typeof(Exhibit) })))
                .Insert(new CodeInstruction(OpCodes.Ldloc_1))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.Gr))))
                .InstructionEnumeration();
        }

    }


    [HarmonyPatch]
    class Gongjuxiang_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Gongjuxiang), "<OnAdded>b__0_0");
        }

        static string ExId() => nameof(Gongjuxiang);

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Stage), nameof(Stage.GetShopToolCards))))
                .Set(OpCodes.Call, AccessTools.Method(typeof(GenerateTools), nameof(GenerateTools.GetTools)))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExhibitSelfRngs), nameof(ExhibitSelfRngs.GetSelfRng), new Type[] { typeof(GameRunController), typeof(string) })))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Gongjuxiang_Patch), nameof(Gongjuxiang_Patch.ExId))))
                .Insert(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GrRngs), nameof(GrRngs.Gr))))
                .InstructionEnumeration();
        }

    }


}
