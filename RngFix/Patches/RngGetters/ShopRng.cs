using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using System.Reflection.Emit;
using LBoL.Core;
using RngFix.CustomRngs;
using LBoL.Base;
using LBoL.Core.Randoms;

namespace RngFix.Patches.RngGetters
{

    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetShopCards))]
    class GetShopCards_Patch
    {
        static RandomGen ReplaceShopRng(GameRunController gr, CardWeightTable cwt)
        {
            var grrngs = GrRngs.GetOrCreate(gr);
            var shopRngs = grrngs.persRngs.shopRngs;

            if (!ShopFactor_Patch.wt_cwt.TryGetValue(cwt, out var cwtName))
            {
                BepinexPlugin.log.LogWarning($"No matching CardWeightTable found. Using fallback rng.");
                return shopRngs.fallbackRng;
            }

            if (!shopRngs.rngs.TryGetValue(cwtName, out var rng))
            {
                BepinexPlugin.log.LogWarning($"No matching cwtName {cwtName} in shop rngs. Using fallback rng");
                return shopRngs.fallbackRng;
            }
            return rng;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.ShopRng), AccessTools.Method(typeof(GetShopCards_Patch), nameof(GetShopCards_Patch.ReplaceShopRng)))
                 .Insert(new CodeInstruction(OpCodes.Ldarg_2))
                 .InstructionEnumeration();
        }
    }



    [HarmonyPatch(typeof(Stage), nameof(Stage.GetShopExhibit))]
    class GetShopExhibit_Patch
    {
        
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                 .ReplaceRngGetter(nameof(GameRunController.ShopRng), AccessTools.Method(typeof(GrRngs), nameof(GrRngs.GetShopExRng)))
                 .InstructionEnumeration();
        }
    }



}
