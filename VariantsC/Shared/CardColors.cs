using LBoL.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace VariantsC.Shared
{
    public static class CardColors
    {
        public readonly static string common = "#a0a1a0";
        public readonly static string uncommon = "#3277d1";
        public readonly static string rare = "#d6cd28";

        public readonly static string basicAndCommon = "#898e89";
        public readonly static string misfortune = "#8a3baf";
        public readonly static string status = "#b25eee";



        public static string ColorName(Card card, string defaultColor = "#000000")
        {
            var name = card.Name + (card.IsUpgraded ? " +" : "");
            if (card.IsBasic && card.Config.Rarity == LBoL.Base.Rarity.Common)
                return name.WrapHex(basicAndCommon);
            switch (card.CardType)
            {
                case LBoL.Base.CardType.Status:
                    return name.WrapHex(status);
                case LBoL.Base.CardType.Misfortune:
                    return name.WrapHex(misfortune);
                default:
                    break;
            }

            switch (card.Config.Rarity)
            {
                case LBoL.Base.Rarity.Common:
                    return name.WrapHex(common);
                case LBoL.Base.Rarity.Uncommon:
                    return name.WrapHex(uncommon);
                case LBoL.Base.Rarity.Shining:
                case LBoL.Base.Rarity.Mythic:
                case LBoL.Base.Rarity.Rare:
                    return name.WrapHex(rare);
                default:
                    break;
            }

            return name.WrapHex(defaultColor);
        }

        public static string WrapHex(this string text, string hex)
        {
            return string.Concat(new string[]
            {
                "<color=",
                hex,
                ">",
                text,
                "</color>"
            });
        }

    }
}
