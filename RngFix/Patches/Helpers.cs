using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Core.Stations;
using RngFix.CustomRngs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches
{
    public static class Helpers
    {
        public static CodeMatcher ReplaceRngGetter(this CodeMatcher matcher, string targetName, MethodInfo newCall)
        {
            var targetGetter = AccessTools.PropertyGetter(typeof(GameRunController), targetName);
            if (targetGetter == null)
                throw new ArgumentException($"Could not find {nameof(GameRunController)}.{targetGetter}");
            var matchFor = new CodeMatch ((ci) => ci.operand as MethodInfo == targetGetter);

            return matcher.MatchForward(false, matchFor)
                 .ThrowIfNotMatch($"{targetName} getter not matched.", matchFor)
                 .Set(OpCodes.Call, newCall);
        }

        public static bool IsActTransition(MapNode node) => 
                   node.StationType is StationType.Entry 
                || node.StationType is StationType.Select
                || node.StationType is StationType.Supply
                || node.StationType is StationType.Trade;


    }
}
