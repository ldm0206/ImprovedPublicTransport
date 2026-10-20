using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ImprovedPublicTransport.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace MileageTaxiServices
{
    /// <summary>
    /// Vanilla computes the taxi drop-off fare as
    /// Mathf.RoundToInt((float)m_transportInfo.m_ticketPrice * distance * 0.001f).
    /// With 32-bit prefab fares the product can exceed int range; Unity's RoundToInt then
    /// returns int.MinValue, and EconomyManager.AddResource subtracts ~2.1 billion from
    /// both the taxi income statistic and the city treasury per passenger. This transpiler
    /// swaps that RoundToInt call for a clamped version.
    /// </summary>
    [HarmonyPatch]
    [UsedImplicitly]
    public static class Patch_TaxiAI_UnloadPassengers
    {
        [UsedImplicitly]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("TaxiAI:UnloadPassengers");
        }

        [HarmonyTranspiler]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var clampedMethod = AccessTools.Method(typeof(Patch_TaxiAI_UnloadPassengers), nameof(RoundFareClamped));
            var replaced = 0;
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call &&
                    instruction.operand is MethodInfo method &&
                    method.DeclaringType == typeof(Mathf) &&
                    method.Name == nameof(Mathf.RoundToInt))
                {
                    // Preserve labels and exception blocks on the swapped instruction.
                    var replacement = new CodeInstruction(OpCodes.Call, clampedMethod)
                    {
                        labels = instruction.labels,
                        blocks = instruction.blocks
                    };
                    replaced++;
                    yield return replacement;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (replaced == 0)
            {
                Utils.LogError("MileageTaxiServices: Mathf.RoundToInt not found in TaxiAI.UnloadPassengers; taxi drop-off fares are unclamped.");
            }
        }

        /// <summary>
        /// Mathf.RoundToInt with int-range clamping: out-of-range and NaN inputs would
        /// otherwise convert to int.MinValue and turn the fare into a massive charge-back.
        /// </summary>
        public static int RoundFareClamped(float rawFare)
        {
            if (float.IsNaN(rawFare) || rawFare <= 0f)
            {
                return 0;
            }
            // (float)int.MaxValue is 2147483648f, so this comparison catches every value
            // that would overflow the int conversion.
            if (rawFare >= 2147483647f)
            {
                return int.MaxValue;
            }
            return Mathf.RoundToInt(rawFare);
        }
    }
}
