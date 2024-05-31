using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.Core;
using LBoL.Core.Cards;
using LBoL.EntityLib.Exhibits.Common;
using LBoL.EntityLib.Exhibits.Shining;
using LBoLEntitySideloader.ReflectionHelpers;
using RngFix.CustomRngs;
using RngFix.Patches.RngGetters;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


namespace RngFix.Patches.Exhibits
{

    [HarmonyPatch]
    class UpgradeRandomCards_Patch
    {

        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Chaidao), "OnGain");
            yield return AccessTools.Method(typeof(Jiaobu), "OnGain");
            yield return AccessTools.Method(typeof(QipaiYouhua), "OnGain");

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Method Declaration", "Harmony003:Harmony non-ref patch parameters modified", Justification = "deeznuts")]
        public static void UpgradeRandomCards(GameRunController gr, int amount = 1, CardType? type = null, Exhibit exhibit = null)
        {
            if (amount <= 0)
            {
                throw new InvalidOperationException("DEEZNUTS");
            }
            List<Card> list = new List<Card>();
            foreach (Card card in gr._baseDeck)
            {
                if (card.CanUpgradeAndPositive)
                {
                    if (type != null)
                    {
                        if (card.CardType == type.Value)
                        {
                            list.Add(card);
                        }
                    }
                    else
                    {
                        list.Add(card);
                    }
                }
            }
            Card[] array = list.SampleManyOrAll(amount, GrRngs.GetOrCreate(gr).ExhibitSelfRngs.GetRng(exhibit));
            gr.UpgradeDeckCards(array, false);
        }


        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
               .MatchForward(true, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(GameRunController), nameof(GameRunController.UpgradeRandomCards))))
               .Set(OpCodes.Call, AccessTools.Method(typeof(UpgradeRandomCards_Patch), nameof(UpgradeRandomCards_Patch.UpgradeRandomCards)))
               .Insert(new CodeInstruction(OpCodes.Ldarg_0))
               .InstructionEnumeration();
        }

    }


}
