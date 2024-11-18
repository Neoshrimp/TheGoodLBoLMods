using LBoL.EntityLib.EnemyUnits.Character;
using LBoL.EntityLib.EnemyUnits.Character.DreamServants;
using LBoL.EntityLib.EnemyUnits.Normal;
using LBoL.EntityLib.EnemyUnits.Normal.Drones;
using LBoL.EntityLib.EnemyUnits.Normal.Guihuos;
using LBoL.EntityLib.EnemyUnits.Normal.Maoyus;
using LBoL.EntityLib.EnemyUnits.Normal.Yinyangyus;
using LBoL.EntityLib.EnemyUnits.Opponent;
using LBoL.EntityLib.Exhibits.Shining;
using System;
using System.Collections.Generic;
using System.Text;

namespace FullElite
{
    public static class VanillaElites
    {

        public static HashSet<string> eliteGroups = new HashSet<string>() { "Sanyue", "Aya", "Rin", "Nitori", "Youmu", "Kokoro", "Clownpiece", "Siji", "Doremy" };


        public static List<Type> act1 = new List<Type>() {
            typeof(Aya),
            typeof(Rin), typeof(GuihuoBlue), typeof(GuihuoRed), typeof(GuihuoGreen),
            typeof(Luna), typeof(Star), typeof(Sunny),
        };
        public static string[] spirits = new string[] { nameof(GuihuoBlue), nameof(GuihuoRed), nameof(GuihuoGreen) };

        public static List<Type> act2 = new List<Type>() {
            typeof(Youmu),
            typeof(Nitori), typeof(PurifierElite), typeof(ScoutElite), typeof(TerminatorElite),
            typeof(Kokoro), typeof(Maoyu), typeof(MaskRed), typeof(MaskBlue), typeof(MaskGreen)
        };

        public static string[] drones = new string[] { nameof(PurifierElite), nameof(ScoutElite), nameof(TerminatorElite) };

        public static string[] masks = new string[] { nameof(MaskRed), nameof(MaskBlue), nameof(MaskGreen) };

        public static List<Type> act3 = new List<Type>() {
            typeof(Siji), typeof(Alice), typeof(Cirno), typeof(Koishi), typeof(Marisa), typeof(Reimu), typeof(Sakuya),
            typeof(Doremy), typeof(DreamAya), typeof(DreamJunko), typeof(DreamRemilia), typeof(DreamYoumu),
            typeof(Clownpiece), typeof(BlackFairy), typeof(WhiteFairy), 
            typeof(YinyangyuBlueReimu), typeof(YinyangyuRedReimu)
        };


        public static string[] dreamGirls = new string[] { nameof(DreamAya), nameof(DreamJunko), nameof(DreamRemilia), nameof(DreamYoumu) };

        public static string[] eikiSummons = new string[] { nameof(Alice), nameof(Cirno), nameof(Koishi), nameof(Marisa), nameof(Reimu), nameof(Sakuya) };

    }
}
