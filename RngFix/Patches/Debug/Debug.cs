using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Core.Randoms;
using LBoL.Presentation;
using LBoL.Presentation.UI.Panels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using static RngFix.BepinexPlugin;

namespace RngFix.Patches.Debug
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
            /*            if (__originalMethod.Name.StartsWith("get_Adventure"))
                            log.LogDebug($"adventureRng state: {__result.State}");*/

            if (__originalMethod.Name.StartsWith("get_CardRng"))
            {
                log.LogDebug($"cardRng state: {__result.State}");
                /*                var st = new StackTrace();

                                log.LogDebug($"---------------");
                                foreach (var f in st.GetFrames())
                                {
                                    log.LogDebug($"{f.GetMethod().DeclaringType.FullName}::{f.GetMethod().Name}@IL_{f.GetNativeOffset().ToString("X4")}");


                                }
                                log.LogDebug($"---------------");*/
            }
        }
    }


    //[HarmonyPatch(typeof(NazrinDetectPanel), nameof(NazrinDetectPanel.Roll))]
    class naz_Patch
    {
        static void Prefix(int resultIndex)
        {

            log.LogDebug($"rez: {resultIndex}");
            var st = new StackTrace();

            log.LogDebug($"---------------");
            foreach (var f in st.GetFrames())
            {
                log.LogDebug($"{f.GetMethod().DeclaringType.FullName}::{f.GetMethod().Name}");
            }
            log.LogDebug($"---------------");
        }
    }


    //[HarmonyPatch(typeof(VnPanel), nameof(VnPanel.RunCommand))]
    class VnPanel_Patch
    {
        static void Prefix(string command, RuntimeCommandHandler extraCommandHandler)
        {
            log.LogDebug($"{command}, {extraCommandHandler}");
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



    [HarmonyPatch]
    //[HarmonyDebug]
    class Up_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            //yield return AccessTools.Method(typeof(UniqueRandomPool<>), "Sample", new Type[] { typeof(Func<float, float, float>) });


            yield return typeof(UniqueRandomPool<>).MakeGenericType(typeof(Card)).GetMethod("Sample", new Type[] { typeof(Func<float, float, float>) });
        }


        static GameRunController Gr() => GameMaster.Instance?.CurrentGameRun;

        static bool CheckSt()
        {
            var st = new StackTrace();
            return st.GetFrames().FirstOrDefault(st => st.GetMethod().Name.StartsWith("RollCards")) != null;
        }

        static void W1(float w)
        {
            if (!CheckSt())
                return;
            log.LogDebug($"state before: {Gr()?.CardRng?.State}");
            log.LogDebug($"total weights: {w}");


        }

        static void W2(float w)
        {
            if (!CheckSt())
                return;
            log.LogDebug($"rolled weight: {w}");
            log.LogDebug($"state after: {Gr()?.CardRng?.State}");


        }


        static void PrintCard(object o, string s)
        {

            if (o is Type type && type.IsSubclassOf(typeof(Card)))
            {
                log.LogDebug(s + type.Name);
                log.LogDebug("-----");

            }
        }

        static void PrintDefault(object o) => PrintCard(o, "(Default roll) ");

        static void PrintCond(object o) => PrintCard(o, "(Conditional roll) ");



        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchForward(true, new CodeInstruction(OpCodes.Stloc_0))
                .Insert(new CodeInstruction(OpCodes.Dup))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Up_Patch), nameof(Up_Patch.W1))))


                .MatchForward(true, new CodeInstruction(OpCodes.Stloc_1))
                .Insert(new CodeInstruction(OpCodes.Dup))
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Up_Patch), nameof(Up_Patch.W2))))

                // 'default' return
                .MatchForward(false, new CodeInstruction(OpCodes.Ret))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Up_Patch), nameof(Up_Patch.PrintDefault))))

                .Advance(1)
                // weight conditional return
                .MatchForward(false, new CodeInstruction(OpCodes.Ret))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Up_Patch), nameof(Up_Patch.PrintCond))))

                .InstructionEnumeration();
        }

    }




}
