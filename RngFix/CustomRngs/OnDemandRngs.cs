using LBoL.Core.Adventures;
using LBoL.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LBoL.Base;
using RngFix.Patches;
using static RngFix.BepinexPlugin;
using System.Diagnostics.CodeAnalysis;

namespace RngFix.CustomRngs
{
    public class OnDemandRngs
    {
        public static string GetId(string str) => str.Substring(Math.Max(0, str.Length - 64), Math.Min(str.Length, 64));

        private static ulong instanceCounter = 0;
        public static ulong InstanceCounter { get => instanceCounter; }

        private Dictionary<string, RandomGen> rngs = new Dictionary<string, RandomGen>();
        public IReadOnlyDictionary<string, RandomGen> Rngs { get => rngs; }

        public readonly ulong seedOffset = 0;

        public OnDemandRngs()
        {
            instanceCounter++;
        }

        public OnDemandRngs(ulong? seedOffset = 0) : this()
        {
            if (seedOffset == null)
                this.seedOffset = instanceCounter - 1;
            else
                this.seedOffset = seedOffset.Value;
        }

        public RandomGen GetOrCreateRootRng(string str, [MaybeNull] ulong? initialState)
        {
            var id = GetId(str);
            //log.LogDebug(id+"_"+seedOffset);

            if (initialState == null)
                initialState = GrRngs.GetOrCreate(GrRngs.Gr()).NodeMaster.rng.State;

            rngs.GetOrCreateVal(
                id,
                () => {
                    ulong seed = 0;
                    unchecked {
                        seed = initialState.Value + UlongHash(id) + seedOffset;
                    }
                    //log.LogDebug(seed);
                    return new RandomGen(seed);
                },
                out var rootRng);

            return rootRng;
        }


        public RandomGen GetSubRng(string str, ulong initialState)
        {
            return new RandomGen(GetOrCreateRootRng(str, initialState).NextULong());
        }



        // FNV-1a 64-bit constants
        private const ulong FNVOffsetBasis = 0xcbf29ce484222325;
        private const ulong FNVPrime = 0x100000001b3;


        public static ulong UlongHash(string str)
        {
            if (str.Length > 64)
                throw new ArgumentException("String length should not exceed 64 characters.");

            ulong hash = FNVOffsetBasis;

            foreach (char c in str)
            {
                hash ^= (byte)c;
                unchecked { hash *= FNVPrime; }

            }

            return hash;
        }

        public static bool IsGameEntity(Type type) => type.IsSubclassOf(typeof(GameEntity)) || type.IsSubclassOf(typeof(Adventure)) || type == typeof(GameEntity) || type == typeof(Adventure);

        /// <summary>
        /// Doesn't return if method is declared as a static in unrelated class
        /// </summary>
        /// <param name="stackDepth"></param>
        /// <returns></returns>
        public static Type FindCallingEntity(int stackDepth = 3, int maxStackDepth = 8)
        {
            Type caller = null;
            Type firstCaller = null;
            bool gameEntityFound = false;


/*
            var st = new StackTrace();
            log.LogDebug($"---------------");
            int i = 0;
            foreach (var f in st.GetFrames())
            {
                log.LogDebug($"{i}:{f.GetMethod().DeclaringType.FullName}::{f.GetMethod().Name}");
                i++;
            }
            log.LogDebug($"---------------");
*/

            int s = stackDepth;
            while (s <= maxStackDepth)
            {
                var stackFrame = new StackFrame(s, false);
                if (stackFrame == null)
                    break;
                var c = stackFrame.GetMethod().DeclaringType;
                while (c != null
                    && c.IsNested
                    && !IsGameEntity(c)
                    )
                    c = c.DeclaringType;

                if (s == stackDepth)
                    firstCaller = c;
                caller = c;
                if (IsGameEntity(c) && c.IsSealed)
                {
                    gameEntityFound = true;
                    break;
                }
                s++;
            }


            // if concrete gameEntity type was not found prefers the first frame walk result
            if (!gameEntityFound)
                caller = firstCaller;

            return caller;
        }

        public static Type FindDeclaringGameEntity(Type type)
        {
            var t = type;
            while (t != null
                && t.IsNested
                && !IsGameEntity(t)
                )
                t = t.DeclaringType;
            return t;
        }
    }
}
