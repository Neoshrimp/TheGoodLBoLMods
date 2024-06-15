using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoLEntitySideloader;
using RngFix.CustomRngs;
using static RngFix.BepinexPlugin;

namespace RngFix.Patches.Debug
{

    //[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollNormalExhibit))]
    //[HarmonyPriority(Priority.High)]
    class ExStat_Patch
    {


        static Dictionary<Type, float> wDic = new Dictionary<Type, float>();

        static void PopulateWDic(float w, Type ex)
        {
            wDic.Add(ex, w);
        }

        static void Prefix()
        {
            wDic.Clear();
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            return new CodeMatcher(instructions, generator)
                .MatchEndForward(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Library), nameof(Library.WeightForExhibit))))
                .MatchEndForward(OpCodes.Mul)
                .Advance(1)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Dup))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))

                .InsertAndAdvance(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExStat_Patch), nameof(ExStat_Patch.PopulateWDic))))


                .InstructionEnumeration();
        }

        static void Postfix(GameRunController __instance, Exhibit __result, RandomGen rng)
        {

            if (wDic.Empty())
                return;

            // DELETE
            //rng.Next();

            var gr = __instance;
            var ex = __result;
            var exLog = StatsLogger.GetExLog(gr);

            exLog.AddVal(ex.Name);
            exLog.AddVal(ex.Config.Rarity);
            exLog.AddVal(ex.Config.Appearance);

            exLog.AddVal(wDic[__result.GetType()]);
            exLog.AddVal(wDic.Values.Sum());

            exLog.AddVal(gr.ExhibitRng.State);
            exLog.AddVal(gr.CurrentStation.Type);


            exLog.FlushVals();


        }

    }


    //[HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollNormalExhibit))]
    class ExConsistentQueue_Patch
    {
        public const int MaxAttempts = (int)1E6;


        static bool Prefix(GameRunController __instance, ref Exhibit __result, RandomGen rng, ExhibitWeightTable weightTable, Func<Exhibit> fallback, Predicate<ExhibitConfig> filter)
        {
            var gr = __instance;
            var grngs = GrRngs.GetOrCreate(gr);
            //var exWrng = GrRngs.GetExhibitWeightRng(gr);
            float totalW = 0f;
            float maxW = 0f;
            var exRollRng = new RandomGen(rng.NextULong());

            //var rollingPool = new RepeatableRandomPool<Type>();
            var rollingPool = new UniqueRandomPool<Type>();


            if (gr.ExhibitPool.Empty())
            { 
                __result = fallback();
                return false;
            }


            Func<Type, float> GetExW = exType => {
                var exConfig = ExhibitConfig.FromId(exType.Name);
                return weightTable.WeightFor(exConfig) * Library.WeightForExhibit(exType, gr);
            };

            var poolSet = new HashSet<Type>(gr.ExhibitPool);


            foreach (var exConfig in ExhibitConfig.AllConfig().Where(c => c.Rarity is Rarity.Common || c.Rarity is Rarity.Uncommon || c.Rarity is Rarity.Rare))
            {
                var exT = TypeFactory<Exhibit>.GetType(exConfig.Id);
                if (exT == null)
                    continue;

                var w = GetExW(exT);
                totalW += w;
                maxW = Math.Max(maxW, w);

                rollingPool.Add(exT, 1f);
            }

            var wThreshold = rng.NextFloat(0, maxW);


            int i = 0;
            while (true)
            {

                var exT = rollingPool.Sample(exRollRng);
                var exConfig = ExhibitConfig.FromId(exT.Name);
                var manaReq = exConfig.BaseManaRequirement;

                if (wThreshold < GetExW(exT)
                    && poolSet.Contains(exT)
                    && (manaReq == null || gr.BaseMana.HasColor(manaReq.GetValueOrDefault()))
                    && (filter == null || filter(exConfig))
                    )
                {
                    log.LogDebug($"Rolled {exT} in attempts {i}");
                    gr.ExhibitPool.Remove(exT);
                    __result = Library.CreateExhibit(exT);
                    break;
                }

                if (i > MaxAttempts)
                {
                    log.LogWarning($"Exceeded max attempts while rolling exhibit. wThresholP: {wThreshold}");
                    //__result = fallback();
                    return true;
                }
                i++;
            }

            var exLog = StatsLogger.GetExLog(gr);
            var ex = __result;

            exLog.AddVal(ex.Name);
            exLog.AddVal(ex.Config.Rarity);
            exLog.AddVal(ex.Config.Appearance);

            exLog.AddVal($"{wThreshold}<{GetExW(ex.GetType())}");
            exLog.AddVal(totalW);

            exLog.AddVal(gr.ExhibitRng.State);
            exLog.AddVal(gr.CurrentStation.Type);


            exLog.FlushVals();

            return false;
        }
    
    }


}
