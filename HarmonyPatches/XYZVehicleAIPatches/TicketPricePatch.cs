using System;
using ColossalFramework;
using HarmonyLib;
using ImprovedPublicTransport.OptionsFramework;
using ImprovedPublicTransport.Util;
using UnityEngine;
using IptSettings = ImprovedPublicTransport.Settings.Settings;

namespace ImprovedPublicTransport.HarmonyPatches.XYZVehicleAIPatches
{
    /// <summary>
    /// The game stores each line's fare in a 16-bit field (TransportLine.m_ticketPrice),
    /// which caps per-line fares at 65,535. The customizer writes the full 32-bit fare to
    /// the transport prefab (TransportInfo.m_ticketPrice, an int). These postfixes make the
    /// passenger AIs return that 32-bit value instead of the line's truncated one.
    ///
    /// The original AI also applies district policies (free transport / high ticket prices);
    /// because the postfix replaces its result, those policies are re-applied here.
    /// </summary>
    public static class TicketPricePatch
    {
        public static void Apply()
        {
            Patch(typeof(BusAI), nameof(BusPostfix));
            Patch(typeof(TrolleybusAI), nameof(TrolleybusPostfix));
            Patch(typeof(TramAI), nameof(TramPostfix));
            Patch(typeof(PassengerTrainAI), nameof(PassengerTrainPostfix));
            Patch(typeof(PassengerShipAI), nameof(PassengerShipPostfix));
            Patch(typeof(PassengerFerryAI), nameof(PassengerFerryPostfix));
            Patch(typeof(PassengerPlaneAI), nameof(PassengerPlanePostfix));
            Patch(typeof(PassengerBlimpAI), nameof(PassengerBlimpPostfix));
            Patch(typeof(PassengerHelicopterAI), nameof(PassengerHelicopterPostfix));
            Patch(typeof(CableCarAI), nameof(CableCarPostfix));
        }

        public static void Undo()
        {
            Unpatch(typeof(BusAI));
            Unpatch(typeof(TrolleybusAI));
            Unpatch(typeof(TramAI));
            Unpatch(typeof(PassengerTrainAI));
            Unpatch(typeof(PassengerShipAI));
            Unpatch(typeof(PassengerFerryAI));
            Unpatch(typeof(PassengerPlaneAI));
            Unpatch(typeof(PassengerBlimpAI));
            Unpatch(typeof(PassengerHelicopterAI));
            Unpatch(typeof(CableCarAI));
        }

        private static void Patch(Type aiType, string postfix)
        {
            PatchUtil.Patch(
                new PatchUtil.MethodDefinition(aiType, "GetTicketPrice"),
                null,
                new PatchUtil.MethodDefinition(typeof(TicketPricePatch), postfix, priority: Priority.Normal)
            );
        }

        private static void Unpatch(Type aiType)
        {
            PatchUtil.Unpatch(new PatchUtil.MethodDefinition(aiType, "GetTicketPrice"));
        }

        public static void BusPostfix(BusAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void TrolleybusPostfix(TrolleybusAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void TramPostfix(TramAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void PassengerTrainPostfix(PassengerTrainAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void PassengerShipPostfix(PassengerShipAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void PassengerFerryPostfix(PassengerFerryAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void PassengerPlanePostfix(PassengerPlaneAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void PassengerBlimpPostfix(PassengerBlimpAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void PassengerHelicopterPostfix(PassengerHelicopterAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        public static void CableCarPostfix(CableCarAI __instance, ref Vehicle data, ref int __result)
        {
            Apply(__instance.m_transportInfo, ref data, ref __result);
        }

        private static void Apply(TransportInfo fallbackInfo, ref Vehicle data, ref int __result)
        {
            // Only take over pricing while the customizer is enabled; otherwise vanilla
            // per-line ticket prices must keep working.
            var options = OptionsWrapper<IptSettings>.Options;
            if (options == null ||
                options.TicketPriceCustomizerMode != (int)IptSettings.TicketPriceCustomizerModes.Enabled)
            {
                return;
            }

            var transportManager = TransportManager.instance;
            if (transportManager == null)
            {
                return;
            }

            TransportInfo info = fallbackInfo;
            if (data.m_transportLine != 0)
            {
                var lineInfo = transportManager.m_lines.m_buffer[data.m_transportLine].Info;
                if (lineInfo != null) info = lineInfo;
            }
            if (info == null)
            {
                return;
            }

            int fare = info.m_ticketPrice;
            // The 16-bit line field can hold this fare, so the original result (which also
            // applied the district policies and any per-line override) is already correct.
            if (fare <= ushort.MaxValue)
            {
                return;
            }

            var districtManager = Singleton<DistrictManager>.instance;
            if (districtManager != null)
            {
                byte district = districtManager.GetDistrict(
                    new Vector3(data.m_targetPos3.x, data.m_targetPos3.y, data.m_targetPos3.z));
                var services = districtManager.m_districts.m_buffer[district].m_servicePolicies;
                if ((services & DistrictPolicies.Services.FreeTransport) != 0)
                {
                    __result = 0;
                    return;
                }

                var eventManager = Singleton<EventManager>.instance;
                if (eventManager != null)
                {
                    var eventPolicies = districtManager.m_districts.m_buffer[district].m_eventPolicies &
                                        eventManager.GetEventPolicyMask();
                    if ((eventPolicies & DistrictPolicies.Event.ComeOneComeAll) != 0)
                    {
                        __result = 0;
                        return;
                    }
                }

                if ((services & DistrictPolicies.Services.HighTicketPrices) != 0)
                {
                    long raised = (long)fare * 5 / 4;
                    fare = (int)Math.Min(int.MaxValue, raised);
                }
            }

            __result = fare;
        }
    }
}
