using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Adventures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using YamlDotNet.Serialization;

namespace VariantsC.Rng
{
    public class PerRngs
    {
        [YamlIgnore]
        static ConditionalWeakTable<GameRunController, PerRngs> table = new ConditionalWeakTable<GameRunController, PerRngs>();

        static public PerRngs Get(GameRunController gr)
        {
            return table.GetOrCreateValue(gr);
        }

        static public void Assign(GameRunController gr, PerRngs persistentRngs)
        {
            table.AddOrUpdate(gr, persistentRngs);
        }



        public void InitRngs(RandomGen rng)
        {
            var subRng = new RandomGen(rng.NextULong());
            everHoardingRng = new RandomGen(subRng.NextULong());
            springCleaningRng = new RandomGen(subRng.NextULong());

        }


        public RandomGen everHoardingRng;
        public RandomGen springCleaningRng;
    }

    [HarmonyPatch]
    [HarmonyPriority(Priority.Low)]
    class Gr_cctor_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Constructor(typeof(GameRunController), new Type[] { typeof(GameRunStartupParameters) });
        }

        static void Postfix(GameRunController __instance)
        {
            PerRngs.Get(__instance).InitRngs(__instance.RootRng);
        }


    }
}
