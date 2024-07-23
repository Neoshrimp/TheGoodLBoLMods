using HarmonyLib;
using LBoL.Base;
using LBoL.Base.Extensions;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.EntityLib.Cards.Character.Koishi;
using LBoL.EntityLib.Cards.Neutral.Blue;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.Exhibits.Shining;
using LBoLEntitySideloader.Entities;
using RngFix.Patches;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using static RngFix.BepinexPlugin;

namespace RngFix.CustomRngs.Sampling.Pads
{
    public static class Padding
    {
        private static IEnumerable<Type> paddedCards = null;
        private static List<(Type cardType, CardConfig config)?> paddedMisfortunes = null;

        private const string lostBin = "LostBin";
        private const string modNeutral = "ModNeutral";

        public static IEnumerable<Type> RewardCards 
        {
            get
            {
                return GetRewardCards(GrRngs.Gr());
            }
        }

        public static List<(Type cardType, CardConfig config)?> PaddedMisfortunes 
        { 
            get 
            {
                if (paddedMisfortunes == null) 
                    paddedMisfortunes = PadMisfortunes().ToList();
                return paddedMisfortunes;
            }  
        }

        public static IEnumerable<Type> GetRewardCards(GameRunController gr)
        {
            if (paddedCards == null)
                paddedCards = CardPadding(CardConfig.AllConfig().Where(cc => cc.IsPooled && cc.DebugLevel <= gr.CardValidDebugLevel));
            return paddedCards;
        }

        

        public static IEnumerable<Type> AllExhibits
        {
            get
            {
                return ExPadding(ExhibitConfig.AllConfig());
            }
        }

        public static IEnumerable<Type> RollableExhibits

        {
            get
            {
                return ExPadding(ExhibitConfig.AllConfig().Where(c => c.Rarity is Rarity.Common || c.Rarity is Rarity.Uncommon || c.Rarity is Rarity.Rare));
            }
        }

        public static void OutputPadding(IEnumerable<Type> padded)
        {
            //log.LogDebug(string.Join(";", BattleRngs.InfiteDeck.Items.Where(t => t != null && !potentialCards.CountedPool.ContainsKey(t)).Select(t => t?.Name ?? "null")));

            /*            var dickSet = new HashSet<Type>(InfiteDeck);

                        log.LogDebug(string.Join(";", potentialCards.CountedPool.Keys.Where(t => !dickSet.Contains(t)).Select(t => t?.Name ?? "null")));*/

            int agg = 0;
            var rez = new List<string>();
            int count = padded.Count();
            foreach ((var t, var i) in padded.Select((t, i) => (t, i)))
            {
                if ((t != null || i == count - 1) && agg > 0)
                {
                    rez.Add($"\n[{agg}]\n");
                    agg = 0;
                }
                if (t != null)
                {
                    rez.Add($"{i}:{t.Name}");
                }
	            if(t == null)
                    agg++;
            }
            Debug.Log(string.Join(";", rez));
        }


        public static IEnumerable<T> EndPadding<T>(int targetSize, IEnumerable<T> collection)
        {

            int padding = targetSize - collection.Count();

            if (padding < 0)
            {
                log.LogWarning($"Target padding {targetSize} exceeded by total {collection.Count()}");
                padding = 0;
            }

            return collection.Concat(new T[padding]);
        }
        public static IEnumerable<T> PadEnd<T>(this IEnumerable<T> collection, int targetSize) => EndPadding(targetSize, collection);

        public static List<Type> SlotByIndex(int max, IEnumerable<KeyValuePair<int, Type>> index2Type, out Dictionary<int, List<Type>> indexDupes)
        {
            indexDupes = new Dictionary<int, List<Type>>();
            var usedIndexes = new HashSet<int>();
            var slots = new List<Type>(Enumerable.Repeat<Type>(null, max));
            foreach ((var k, var v) in index2Type)
            {
                if (k >= slots.Count || k < 0)
                {
                    log.LogError($"{v.Name} not pooled. {k} is out of range {max}");
                    continue;
                }
                var isDupe = usedIndexes.Contains(k);
                usedIndexes.Add(k);

                if (!isDupe)
                    slots[k] = v;
                else
                { 
                    indexDupes.GetOrCreateVal(k, () => new List<Type>(), out var iL);
                    iL.Add(v);
                }

            }
            //log.LogDebug(string.Join(";", indexDupes.Select(kv => $"{kv.Key}:{kv.Value.Count},{kv.Value.First().Name}")));
            return slots;
        }


        public static List<ManaColor?> PadManaColours(IEnumerable<ManaColor> toPad, bool inverse = false, int groupSize = 20, int leftOverSize = 20)
        {
            var colourCount = Enum.GetValues(typeof(ManaColor)).Length;

            var rez = new List<ManaColor?>(Enumerable.Repeat<ManaColor?>(null, colourCount * groupSize));
            var cIndexes = new Dictionary<ManaColor, int>();
            var leftOvers = new List<ManaColor?>();
            foreach (var c in toPad)
            {
                cIndexes.TryAdd(c, 0);
                if (cIndexes[c] >= groupSize)
                {
                    leftOvers.Add(c);
                    continue;
                }

                var i = !inverse ? cIndexes[c] + (int)c * groupSize : -(cIndexes[c]+1) + ((int)c + 1) * groupSize;
                rez[i] = c;
                cIndexes[c]++;
            }

            if (!inverse)
                rez.AddRange(leftOvers.OrderBy(c => (int)c).PadEnd(leftOverSize));
            else
            { 
                rez.AddRange(Enumerable.Repeat<ManaColor?>(null, Math.Max(0, leftOverSize - leftOvers.Count)));
                rez.AddRange(leftOvers);
            }

            return rez;
        }


        public static IEnumerable<Type> ExPadding(IEnumerable<ExhibitConfig> validExhibits, int total = 2000, int reserveForDupes = 50)
        {

            var vanilla = SlotByIndex(800, validExhibits
                .Where(ec => ec.Index < 800)
                .Select(ec => new KeyValuePair<int, Type>(ec.Index, TypeFactory<Exhibit>.TryGetType(ec.Id)))
                .Where(tu => tu.Value != null)
                , out var indexDupes
                );

            vanilla = vanilla.Concat(validExhibits.Where(ec => ec.Index >= 800)
                        .Select(ec => TypeFactory<Exhibit>.TryGetType(ec.Id))
                        .Where(t => t != null)
                        ).ToList();


            var rez = vanilla.AsEnumerable();
            rez = rez.PadEnd(total - reserveForDupes);

            if (!indexDupes.Empty())
            {
                rez = rez.Concat(indexDupes.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value));
            }

            return EndPadding(total, vanilla);

        }

        public static IEnumerable<Type> AllAdventurePadding(int total = 300, int reserveForDupes = 50)
        {
            var adventures = AdventureConfig.AllConfig().OrderBy(ac => ac.No);
            var vanilla = SlotByIndex(100, adventures.Where(ac => ac.No < 100)
                            .Select(ac => new KeyValuePair<int, Type>(ac.No, TypeFactory<Adventure>.TryGetType(ac.Id)))
                            .Where(tu => tu.Value != null)
                            , out var indexDupes
                            );

            vanilla = vanilla.Concat(adventures.Where(ac => ac.No >= 100)
                            .Select(ac => TypeFactory<Adventure>.TryGetType(ac.Id))
                            .Where(t => t != null)
                            ).ToList();


            var rez = vanilla.AsEnumerable();
            rez = rez.PadEnd(total - reserveForDupes);

            if (!indexDupes.Empty())
            {
                rez = rez.Concat(indexDupes.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value));
            }

            return EndPadding(total, vanilla);
        }

        public static IEnumerable<(Type cardType, CardConfig config)?> PadMisfortunes(int vanillaBuffer = 200, int modBuffer = 300)
        {
            return Library.EnumerateCardTypes()
                .Where(tu => tu.config.Type == CardType.Misfortune)
                .OrderBy(tu => tu.config.Index)
                .Select(tu => new Nullable<(Type cardType, CardConfig config)>((tu.cardType, tu.config)))
                .GroupBy(tu => tu.Value.config.Index >= 10000)
                .Select((g, i) => i == 0 ? g.PadEnd(vanillaBuffer) : g.AsEnumerable())
                .Aggregate((l1, l2) => l1.Concat(l2))
                .PadEnd(vanillaBuffer + modBuffer);
        }


        public static IEnumerable<Type> CardPadding(IEnumerable<CardConfig> validCards, int total = (int)10E3, int charSize = 200, int charBuffer = 2400, int moddedBuffer = 2000, int lostBuffer = 400)
        {
            validCards = validCards.OrderBy(cc => cc.Index);

            var minIndex = validCards.First().Index;

            var vanilla = SlotByIndex(5000, validCards.Where(cc => cc.Index < 5000)
                .Select(cc => new KeyValuePair<int, Type>(cc.Index, TypeFactory<Card>.TryGetType(cc.Id)))
                .Where(tu => tu.Value != null)
                , out var indexDupes
                );

            bool dupesHandled = false;
            if (!indexDupes.Empty() && indexDupes.Sum(kv => kv.Value.Count) < minIndex)
            {
                int vi = 0;
                foreach (var kv in indexDupes.OrderBy(kv => kv.Key))
                {
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        vanilla[vi] = kv.Value[i];
                        vi++;
                    }
                }
                dupesHandled = true;
            }

            var bins = new Dictionary<string, List<Type>>();
            var moddedOrder = new List<string>();

            var restOfTheCards = validCards.Where(cc => cc.Index >= 5000);

            foreach (var cc in restOfTheCards)
            {
                string binKey = "";
                var cType = TypeFactory<Card>.TryGetType(cc.Id);
                if (cType == null)
                    continue;

                if (cc.Index >= 10000)
                {
                    if (string.IsNullOrEmpty(cc.Owner))
                        binKey = modNeutral;
                    else
                    {
                        binKey = cc.Owner;
                        if (moddedOrder.LastOrDefault() != cc.Owner) // assuming mfs are not messing with index
                            moddedOrder.Add(cc.Owner);
                    }
                }
                else
                {
                    binKey = lostBin;
                }


                bins.TryAdd(binKey, new List<Type>());
                bins[binKey].Add(cType);
            }


            var moddedChars = new List<Type>();
            foreach (var binKey in moddedOrder)
            {
                if (!bins.TryGetValue(binKey, out var bin))
                    continue;

                moddedChars.AddRange(EndPadding(charSize, bin));
            }

            moddedChars = EndPadding(charBuffer, moddedChars).ToList();

            var rez = vanilla.Concat(moddedChars);

            List<Type> moddedNeutrals;
            bins.TryGetValue(modNeutral, out moddedNeutrals);
            moddedNeutrals ??= new List<Type>();
            rez = rez.Concat(EndPadding(moddedBuffer, moddedNeutrals));

            List<Type> lost;
            bins.TryGetValue(lostBin, out lost);
            lost ??= new List<Type>();
            rez = rez.Concat(lost.PadEnd(lostBuffer));

            if(!dupesHandled)
            {
                rez = rez.Concat(indexDupes.OrderBy(kv => kv.Key).SelectMany(kv => kv.Value));
            }

            rez = EndPadding(total, rez);

            return rez;
        }



        [HarmonyPatch(typeof(CardConfig), nameof(CardConfig.Load))]
        [HarmonyPriority(Priority.High)]
        class FixTaoFetusIndex_Patch
        {
            static void Postfix()
            {
                var cc = CardConfig.FromId(nameof(QingeDraw));
                if (cc.Index == 1206)
                    cc.Index = 1208;
            }
        }


        [HarmonyPatch]
        class WarmUpPadding_Create_Patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Constructor(typeof(GameRunController), new Type[] { typeof(GameRunStartupParameters) });
            }

            static void Postfix(GameRunController __instance)
            {
                GetRewardCards(__instance);
                var __ = BattleRngs.PaddedCards;
                var _ = PaddedMisfortunes;
            }
        }


        [HarmonyPatch]
        class WarmUpPadding_Restore_Patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(GameRunController), nameof(GameRunController.Restore));
            }

            static void Postfix(GameRunController __result)
            {
                GetRewardCards(__result);
                var __ = BattleRngs.PaddedCards;
                var _ = PaddedMisfortunes;
            }
        }

    }
}
