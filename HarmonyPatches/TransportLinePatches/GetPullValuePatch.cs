using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ImprovedPublicTransport.Util;

namespace ImprovedPublicTransport.HarmonyPatches.TransportLinePatches
{
    /// <summary>
    /// Keeps the line's "pull" value (its displayed attractiveness) independent of the
    /// ticket price, matching the simulation patch that removes the fare's effect on
    /// ridership. Without this the panel would still show fares lowering the pull.
    /// </summary>
    public class GetPullValuePatch
    {
        public static void Apply()
        {
            PatchUtil.Patch(
                new PatchUtil.MethodDefinition(typeof(TransportLine), nameof(TransportLine.GetPullValue)),
                null,
                null,
                new PatchUtil.MethodDefinition(typeof(GetPullValuePatch), nameof(Transpile))
            );
        }

        public static void Undo()
        {
            PatchUtil.Unpatch(
                new PatchUtil.MethodDefinition(typeof(TransportLine), nameof(TransportLine.GetPullValue))
            );
        }

        public static IEnumerable<CodeInstruction> Transpile(MethodBase original,
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>();
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld &&
                    instruction.operand is FieldInfo fareField &&
                    fareField.DeclaringType == typeof(TransportLine) &&
                    fareField.Name == nameof(TransportLine.m_ticketPrice))
                {
                    codes.Add(new CodeInstruction(OpCodes.Ldc_I4_0) { labels = instruction.labels });
                    continue;
                }

                codes.Add(instruction);
            }

            return codes;
        }
    }
}
