using HarmonyLib;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using static RngFix.BepinexPlugin;

namespace RngFix.Patches
{

    //[HarmonyPatch]
    class RngGetDebug_Patch
    {


        static IEnumerable<MethodBase> TargetMethods()
        {
            var targets = typeof(GameRunController).GetProperties().Where(pi => pi.Name.EndsWith("Rng")).Select(pi => pi.GetMethod);


            return targets;

        }


        static void Postfix()
        {

        }
    }


}
