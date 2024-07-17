using HarmonyLib;
using LBoL.Core;
using LBoLEntitySideloader;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using System.Threading;
using static RngFix.BepinexPlugin;


namespace RngFix.CustomRngs
{
    public class EntityToCallchain
    {
        static Dictionary<string, GameEntity> properties = new Dictionary<string, GameEntity>();

        public static IReadOnlyDictionary<string, GameEntity> Properties { get => properties; }


        public static string GetId(string caller) => caller + "_" + Thread.CurrentThread.ManagedThreadId;

        public static string GetId(MethodBase method) => GetId(method.FullDescription());

        public static void Attach(string caller, GameEntity entity, bool warn = true)
        {
            var id = GetId(caller);
            var wasPresent = properties.AlwaysAdd(id, entity);
            if (warn && wasPresent)
                log.LogWarning($"{id}:{entity.Name} was not consumed");
        }

        [return: MaybeNull]
        public static GameEntity Consume(string caller)
        {
            var id = GetId(caller);

            if (!properties.TryGetValue(id, out var entity))
                log.LogWarning($"{id} is not attached");
            else
                properties.Remove(id);

            return entity;
        }

        public static bool TryConsume(string caller, out GameEntity entity)
        {
            var id = GetId(caller);
            entity = null;
            if (!properties.ContainsKey(id))
                return false;
            entity =  Consume(caller);
            return true;
        }
    }
}
