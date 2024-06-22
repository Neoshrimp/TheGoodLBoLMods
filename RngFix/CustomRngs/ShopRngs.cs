using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.Presentation.UI.ExtraWidgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RngFix.CustomRngs
{
    public class ShopRngs
    {
        public Dictionary<string, RandomGen> rngs = new Dictionary<string, RandomGen>();

        public RandomGen exRng;

        public RandomGen fallbackRng;

        public ShopRngs() { }

        public static ShopRngs Init(ulong seed)
        {
            var shopRngs = new ShopRngs();
            var initRng = new RandomGen(seed);

            shopRngs.exRng = new RandomGen(initRng.Next());
            shopRngs.fallbackRng = new RandomGen(initRng.Next());

            AccessTools.GetDeclaredProperties(typeof(Stage))
                       .Where(pi => pi.GetMethod != null && pi.GetMethod.ReturnType == typeof(CardWeightTable) && pi.GetMethod.Name.StartsWith("get_Shop") && pi.GetMethod.Name.EndsWith("Weight"))
                       .Select(pi => pi.GetMethod.Name).OrderBy(s => s, StringComparer.Ordinal)
                       .Do(s => shopRngs.rngs.Add(s, new RandomGen(initRng.Next())));


            return shopRngs;
        }
    }
}
