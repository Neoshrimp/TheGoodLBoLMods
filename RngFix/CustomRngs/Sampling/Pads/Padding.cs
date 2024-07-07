using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Adventures;
using LBoL.Core.Cards;
using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.Exhibits.Shining;
using LBoLEntitySideloader.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine.Rendering;
using static RngFix.BepinexPlugin;

namespace RngFix.CustomRngs.Sampling.Pads
{
    public static class Padding
    {
        private static IEnumerable<Type> paddedCards = null;


        private const string lostBin = "LostBin";
        private const string modNeutral = "ModNeutral";

        public static IEnumerable<Type> RewardCards 
        {
            get
            {
                if (paddedCards == null)
                    paddedCards = CardPadding(CardConfig.AllConfig().Where(cc => cc.IsPooled && cc.DebugLevel <= GrRngs.Gr().CardValidDebugLevel));
                return paddedCards;
            }
        }


        public static IEnumerable<T> EndPadding<T>(int targetSize, IEnumerable<T> collection) where T : class
        {

            int padding = targetSize - collection.Count();

            if (padding < 0)
            {
                log.LogWarning($"Target padding {targetSize} exceeded by total {collection.Count()}");
                padding = 0;
            }

            return collection.Concat(Enumerable.Repeat<T>(null, padding));
        }
        public static IEnumerable<T> PadEnd<T>(this IEnumerable<T> collection, int targetSize) where T : class => EndPadding<T>(targetSize, collection);

        public static List<Type> SlotByIndex(int max, IEnumerable<KeyValuePair<int, Type>> index2Type)
        {
            var slots = new List<Type>(Enumerable.Repeat<Type>(null, max));
            foreach ((var k, var v) in index2Type)
            {
                if (k >= slots.Count || k < 0)
                {
                    log.LogError($"{v.Name} not pooled. {k} is out of range {max}");
                }
                slots[k] = v;
            }
            return slots;
        }

        public static IEnumerable<Type> ExPadding(int total = 2000)
        {
            var validExhibits = ExhibitConfig.AllConfig().Where(c => c.Rarity is Rarity.Common || c.Rarity is Rarity.Uncommon || c.Rarity is Rarity.Rare);

            var vanilla = SlotByIndex(800, validExhibits
                .Where(ec => ec.Index < 800)
                .Select(ec => new KeyValuePair<int, Type>(ec.Index, TypeFactory<Exhibit>.TryGetType(ec.Id)))
                .Where(tu => tu.Value != null)
                );

            vanilla = vanilla.Concat(validExhibits.Where(ec => ec.Index >= 800)
                        .Select(ec => TypeFactory<Exhibit>.TryGetType(ec.Id))
                        .Where(t => t != null)
                        ).ToList();

            return EndPadding(total, vanilla);

        }

        public static IEnumerable<Type> AdventurePadding(int total = 300)
        {
            var adventures = AdventureConfig.AllConfig().OrderBy(ac => ac.No);
            var vanilla = SlotByIndex(100, adventures.Where(ac => ac.No < 100)
                            .Select(ac => new KeyValuePair<int, Type>(ac.No, TypeFactory<Adventure>.TryGetType(ac.Id)))
                            .Where(tu => tu.Value != null)
                            );

            vanilla = vanilla.Concat(adventures.Where(ac => ac.No >= 100)
                            .Select(ac => TypeFactory<Adventure>.TryGetType(ac.Id))
                            .Where(t => t != null)
                            ).ToList();

            return EndPadding(total, vanilla);
        }


        public static IEnumerable<Type> CardPadding(IEnumerable<CardConfig> validCards, int total = (int)10E3, int charSize = 200, int charBuffer = 2400, int moddedBuffer = 2000)
        {
            validCards = validCards.OrderBy(cc => cc.Index);

            var vanilla = SlotByIndex(5000, validCards.Where(cc => cc.Index < 5000)
                .Select(cc => new KeyValuePair<int, Type>(cc.Index, TypeFactory<Card>.TryGetType(cc.Id)))
                .Where(tu => tu.Value != null)
                );

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

            if (bins.TryGetValue(modNeutral, out var moddedNeutrals))
                rez = rez.Concat(EndPadding(moddedBuffer, moddedNeutrals));

            if (bins.TryGetValue(modNeutral, out var lost))
                rez = rez.Concat(lost);

            rez = EndPadding(total, rez);

            return rez;
        }
    }
}
