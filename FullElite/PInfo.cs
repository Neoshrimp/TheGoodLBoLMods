using HarmonyLib;

namespace FullElite
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.runmods.fullElite";
        public const string Name = "Full Elite";
        public const string version = "1.1.70";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
