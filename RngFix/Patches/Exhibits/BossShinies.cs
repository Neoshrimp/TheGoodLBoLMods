using HarmonyLib;
using LBoL.Base;
using LBoL.ConfigData;
using LBoL.Core;
using LBoL.Core.Randoms;
using LBoL.EntityLib.Cards.Character.Cirno;
using LBoL.EntityLib.Exhibits.Shining;
using RngFix.Patches.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RngFix.Patches.Exhibits
{


    [HarmonyPatch(typeof(GameRunController), nameof(GameRunController.RollShiningExhibit))]
    class RollShiningExhibit_Patch
    {
        public const float maxTotalWeight = 200f;

        private static Type ConsistentExSample(RepeatableRandomPool<Type> pool, RandomGen rng, GameRunController gr)
        {
            var subRng = new RandomGen(rng.NextULong());

            var rollingPool = new RepeatableRandomPool<Type>(pool);
            var poolTotalW = pool._entries.Sum(e => e.Weight);
            if (maxTotalWeight > poolTotalW)
                rollingPool.Add(null, maxTotalWeight - poolTotalW);
            else
                BepinexPlugin.log.LogWarning($"Total shining exhibit weight {poolTotalW} exceed {maxTotalWeight}");

            Type rez = null;
            int i = 0;
            int maxAttempts = (int)1E6;
            while (rez == null && i < maxAttempts)
            {
                rez = rollingPool.Sample(subRng);

                i++;
            }
            if (rez == null)
            {
                rez = pool.Sample(subRng);
                BepinexPlugin.log.LogWarning($"Shinning exhibit not sampled in {i} rolls. Using fallback.");
            }

            return rez;
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                        .SearchForward(ci => ci.opcode == OpCodes.Callvirt && ci.operand is MethodBase mb && (mb.Name == "Sample"))
                        .ThrowIfInvalid("")
                        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(RollShiningExhibit_Patch), nameof(RollShiningExhibit_Patch.ConsistentExSample))))
                        .Insert(new CodeInstruction(OpCodes.Ldarg_0))
                        .InstructionEnumeration();
        }


    }

}
