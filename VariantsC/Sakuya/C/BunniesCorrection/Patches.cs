using HarmonyLib;
using LBoL.Core;
using LBoL.Core.Battle;
using LBoL.Core.Battle.BattleActions;
using LBoL.Core.Battle.Interactions;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Enemy;
using LBoL.Presentation;
using LBoLEntitySideloader.ExtraFunc;
using LBoLEntitySideloader.ReflectionHelpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace VariantsC.Sakuya.C.BunniesCorrection
{

    [HarmonyPatch]
    class MoonTipsBag_Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return ExtraAccess.InnerMoveNext(typeof(MoonTipsBag), nameof(MoonTipsBag.Actions));
        }

        public static bool DoPatch()
        {
            return GameMaster.Instance?.CurrentGameRun?.Player?.HasExhibit<BloodyRipperEx>() ?? false;
        }

        static void CreateNewOption(List<Card> list)
        {
            if(DoPatch())
                list.Add(Library.CreateCard<MoonTipsSpacesuitCard>());
        }


        static bool CheckOption(Card card, MoonTipsBag moonTipsBag)
        {
            if (card is MoonTipsSpacesuitCard)
            {
                // does not reduce the first lifeloss but w/e
                moonTipsBag.Battle.RequestDebugAction(moonTipsBag.BuffAction<MoonTipsSpacesuitSe>(), "Special Moon Tip") ;
                return true;
            }
            return false;
        }



        static IEnumerable<CodeInstruction> Transpiler(MethodBase original, IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            

            return new CodeMatcher(instructions, generator)
                .End()
                .MatchBack(false, new CodeMatch[] { OpCodes.Ldarg_0 } )
                .CreateLabel(out var finalReturn)

                .Start()
                .MatchForward(true, new CodeMatch[] {
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Library), nameof(Library.CreateCard), new Type[] { } ).MakeGenericMethod(new Type[] {typeof(MoonTipsHeal)}))
                })
                .Advance(2)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MoonTipsBag_Patch), nameof(MoonTipsBag_Patch.CreateNewOption))))
                .MatchForward(false, new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(MiniSelectCardInteraction), nameof(MiniSelectCardInteraction.SelectedCard))))
                .Advance(2)

                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MoonTipsBag_Patch), nameof(MoonTipsBag_Patch.CheckOption))))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue, finalReturn))

                .InstructionEnumeration();
        }

    }





}
