using HarmonyLib;

namespace RngFix
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.fix.rngFix";
        public const string Name = "RngFix";
        public const string version = "2.3.2";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
