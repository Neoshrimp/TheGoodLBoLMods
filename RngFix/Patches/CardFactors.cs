using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoLEntitySideloader.Utils;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace RngFix.Patches
{

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.BaseCardWeight))]
    class KillCardFactors_Patch
    {

        static bool IgnoreFactorTable(bool factorFound)
        {
            if (BepinexPlugin.ignoreFactorsTableConf.Value)
                return false;
            return factorFound;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameRunController), nameof(GameRunController._cardRewardWeightFactors))))
                .MatchForward(false, new CodeMatch(OpCodes.Brfalse))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(KillCardFactors_Patch), nameof(KillCardFactors_Patch.IgnoreFactorTable))))
                .LeaveJumpFix().InstructionEnumeration();
        }
    }


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetShopCards))]
    class GetShopCards_Patch
    {
        static int IgnoreFactorTable() => BepinexPlugin.ignoreFactorsTableConf.Value ? 0 : 1;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            return new CodeMatcher(instructions)
                .SearchForward((ci) => ci.operand is MethodBase mb && mb.Name == "RollCards")
                .MatchBack(false, new CodeMatch[] { OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_0 })
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GetShopCards_Patch), nameof(GetShopCards_Patch.IgnoreFactorTable))))
                .LeaveJumpFix().InstructionEnumeration();
        }
    }


    [HarmonyPatch]
    public class ShopFactor_Patch
    {

        public static ConditionalWeakTable<CardWeightTable, string> wt_cwt = new ConditionalWeakTable<CardWeightTable, string>();

        public const float constRareFactor = 0.93f;

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Stage), nameof(Stage.GetShopNormalCards));
            yield return AccessTools.Method(typeof(Stage), nameof(Stage.SupplyShopCard));
            yield return AccessTools.Method(typeof(Stage), nameof(Stage.GetShopToolCards));
        }



        static CardWeightTable ModCardWTable(CardWeightTable cwt, string cwtName)
        {
            var rez = cwt;
            if (BepinexPlugin.ignoreFactorsTableConf.Value)
            { 
                var rar = cwt.RarityTable;
                rez = new CardWeightTable(new RarityWeightTable(rar.Common, rar.Uncommon, rar.Rare * constRareFactor, rar.Mythic), cwt.OwnerTable, cwt.CardTypeTable, cwt.IncludeOutsideKeyword);
            }

            wt_cwt.AddOrUpdate(rez, cwtName);
            return rez;
        }



        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var matcher = new CodeMatcher(instructions).Start();
            
            while (matcher.IsValid)
            {
                matcher = matcher.SearchForward(ci => {

                    return (ci.opcode == OpCodes.Call || ci.opcode == OpCodes.Callvirt) && ci.operand is MethodBase mb && mb.Name.StartsWith("get_Shop") && mb.Name.EndsWith("Weight");
                }
                );
                if (matcher.IsValid)
                {
                    var cwtName = (matcher.Instruction.operand as MethodBase).Name;
                    matcher = matcher
                        .Advance(1)
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Ldstr, cwtName))
                        .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ShopFactor_Patch), nameof(ShopFactor_Patch.ModCardWTable))));
                }
            }

            return matcher.LeaveJumpFix().InstructionEnumeration();
        }


    }




}
