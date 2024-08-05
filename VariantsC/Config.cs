using DG.Tweening.Plugins;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using VariantsC.Marisa.C;
using VariantsC.Reimu.C;
using VariantsC.Rng;
using VariantsC.Sakuya.C;
using static VariantsC.BepinexPlugin;

namespace VariantsC
{
    public class Config
    {

        [HarmonyPatch]
        class Gr_cctor_Patch
        {

            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Constructor(typeof(GameRunController), new Type[] { typeof(GameRunStartupParameters) });
            }

            static void Postfix(GameRunController __instance)
            {
                if(poolNewExhibits.Value)
                    __instance.ShiningExhibitPool.RemoveAll(t => cExhibits.Contains(t));


            }


        }

        public static readonly Type[] cExhibits = new Type[] { typeof(ChocolateCoinEx), typeof(BloodyRipperEx), typeof(PachyBagEx) };


        public static readonly string[][] newCards = new string[][] {
            new string[] { nameof(RollingPebbleCard) },
            new string[] { nameof(ConsequenceOfHickeysCard), nameof(ChaoticBloodMagicCard) },
            new string[] { nameof(EverHoardingCard) }
        };



        [HarmonyPatch(typeof(Library), nameof(Library.EnumerateRollableCardTypes), MethodType.Enumerator)]
        [HarmonyDebug]
        class EnumerateRollableCardTypes_Patch
        {
            private static int CheckConfig(CardType cardType, CardConfig config)
            {
                if(cardType == CardType.Misfortune)
                    return 0;
                var gr = GameMaster.Instance.CurrentGameRun;
                if (gr != null && !poolNewCards.Value)
                {
                    if (newCards.SelectMany(l => l).Contains(config.Id))
                    {
                        return gr.Player.Exhibits
                            .Select(ex => Array.IndexOf(cExhibits, ex.GetType()))
                            .Where(i => i >= 0)
                            .SelectMany(i => newCards[i])
                            .Contains(config.Id) ? 1 : 0;
                    }
                }
                return 1;
            }

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                return new CodeMatcher(instructions)
                    .MatchEndForward(new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CardConfig), nameof(CardConfig.Type))))
                    .MatchEndForward(OpCodes.Ldloc_S)
                    .Advance(1)
                    .RemoveInstruction()
                    //.SetAndAdvance(OpCodes.Dup, null)
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_2))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnumerateRollableCardTypes_Patch), nameof(EnumerateRollableCardTypes_Patch.CheckConfig))))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_0))
                    .InstructionEnumeration();
            }

        }





    }
}
