using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using static RngFix.BepinexPlugin;

namespace RngFix.Patches
{

    [HarmonyPatch]
    class RngGetDebug_Patch
    {

        static MethodInfo adventureRng = AccessTools.PropertyGetter(typeof(GameRunController), nameof(GameRunController.AdventureRng));


        static IEnumerable<MethodBase> TargetMethods()
        {
            var targets = typeof(GameRunController).GetProperties().Where(pi => pi.Name.EndsWith("Rng")).Select(pi => pi.GetMethod);


            return targets;

        }


/*        static void Prefix(MethodInfo originalMethod)
        {
            if (originalMethod != adventureRng)
                return;

        }*/

        static void Postfix(MethodBase __originalMethod, RandomGen __result)
        {
            if (__originalMethod.Name.StartsWith("get_Adventure"))
                log.LogDebug($"adventureRng state: {__result.State}");

            if (__originalMethod.Name.StartsWith("get_CardRng"))
                log.LogDebug($"cardRng state: {__result.State}");
        }
    }


/*    [HarmonyPatch(typeof(RandomGen), nameof(RandomGen.NextInt))]
    class RandomGen_Patch
    {
        static void Prefix()
        {

        }
        static void Postfix(RandomGen __instance)
        {
            var st = new StackTrace();
            bool isSampleMany = false;
            bool isAdventureRng = false;

            log.LogDebug($"---------------");
            foreach (var f in st.GetFrames())
            {
                log.LogDebug($"{f.GetMethod().Name}");

                if (f.GetMethod().Name.StartsWith("SampleMany"))
                    isSampleMany = true;
                if (f.GetMethod().Name.StartsWith("get_Adventure"))
                    isAdventureRng = true;
            }
            log.LogDebug($"---------------");

            if (isSampleMany && isAdventureRng)
                log.LogDebug($"adv rng call state: {__instance.State}");


        }
    }*/




}
