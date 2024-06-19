using HarmonyLib;
using LBoL.Base;
using LBoL.Core;
using LBoL.Core.Cards;
using Logging;
using System;
using System.Collections.Generic;
using System.Text;
using LBoL.Presentation;
using LBoLEntitySideloader.CustomHandlers;
using System.Linq;

namespace RngFix.Patches.Debug
{





    class PickCardLog
    {
        public static void RegisterOnCardsAdded()
        {
            
            CHandlerManager.RegisterGameEventHandler((gr) => gr.DeckCardsAdded, args =>
            {
                StatsLogger.LogPickedCard(args.Cards.First(), args.Cards.Length, StatsLogger.Gr());
            });
        }
    }



}
