﻿using HarmonyLib;

namespace VariantsC
{
    public static class PInfo
    {
        // each loaded plugin needs to have a unique GUID. usually author+generalCategory+Name is good enough
        public const string GUID = "neo.lbol.gameplay.VariantsC";
        public const string Name = "Variants C";
        public const string version = "0.0.4200";
        public static readonly Harmony harmony = new Harmony(GUID);

    }
}
