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


namespace RngFix.Patches.Cards
{
    // AGGRO PREFIX FALSE
    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.GetRandomCurseCard))]
    [HarmonyPriority(Priority.Low)]
    public class GetRandomCurseCard_Patch
    {

        public static bool Prefix(GameRunController __instance, RandomGen rng, bool containUnremovable, ref Card __result)
        {
            var pool = Padding.PaddedMisfortunes.ToList();
            var subRng = new RandomGen(rng.NextULong());

            pool.Shuffle(subRng);

            var cType = pool.FirstOrDefault(tu => tu != null && (containUnremovable || !tu.Value.config.Keywords.HasFlag(Keyword.Unremovable)))?.cardType;

            __result = null;
            if (cType != null)
            { 
                __result = TypeFactory<Card>.CreateInstance(cType);
                __result.GameRun = __instance;
            }

            return false;
        }
    }

}
