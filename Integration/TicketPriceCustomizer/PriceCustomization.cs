using System;
using ColossalFramework;
using ColossalFramework.IO;
using UnityEngine;
using ImprovedPublicTransport.OptionsFramework;
using ImprovedPublicTransport.Util;
using IPTUtils = ImprovedPublicTransport.Util.Utils;

namespace ImprovedPublicTransport.Integration.TicketPriceCustomizer
{
    public class PriceCustomization
    {
        /// <summary>
        /// Overload that accepts nested Settings.TicketPriceCustomizerSettings for consolidated configuration.
        /// </summary>
        public static void SetPrices(ImprovedPublicTransport.Settings.Settings.TicketPriceCustomizerSettings settings)
        {
            // Always apply ticket customizer when requested. IPT manages whether this integration is active.
            if (settings == null) return;

            bool hasAfterDark    = SteamHelper.IsDLCOwned(SteamHelper.DLC.AfterDarkDLC);
            bool hasSnowfall     = SteamHelper.IsDLCOwned(SteamHelper.DLC.SnowFallDLC);
            bool hasMassTransit  = SteamHelper.IsDLCOwned(SteamHelper.DLC.InMotionDLC);
            bool hasSunsetHarbor = SteamHelper.IsDLCOwned(SteamHelper.DLC.UrbanDLC);  // Sunset Harbor = UrbanDLC
            bool hasParklife     = SteamHelper.IsDLCOwned(SteamHelper.DLC.ParksDLC);

            SetBusPrice(settings.BusMultiplier);
            SetMetroPrice(settings.MetroMultiplier);
            SetTrainPrice(settings.TrainMultiplier);
            SetShipPrice(settings.ShipMultiplier);
            SetPlanePrice(settings.PlaneMultiplier);
            if (hasAfterDark)    SetTaxiPrice(settings.TaxiMultiplier);
            if (hasSnowfall)     SetTramPrice(settings.TramMultiplier);
            if (hasMassTransit)  SetBlimpPrice(settings.BlimpMultiplier);
            if (hasMassTransit)  SetFerryPrice(settings.FerryMultiplier);
            if (hasMassTransit)  SetCableCarPrice(settings.CableCarMultiplier);
            if (hasMassTransit)  SetMonorailPrice(settings.MonorailMultiplier);
            if (hasParklife)     SetSightseeingBusPrice(settings.SightseeingBusMultiplier);
            if (hasSunsetHarbor) SetIntercityBusPrice(settings.IntercityBusMultiplier);
            if (hasSunsetHarbor) SetHelicopterPrice(settings.HelicopterMultiplier);
            if (hasSunsetHarbor) SetTrolleybusPrice(settings.TrolleybusMultiplier);
        }

        /// <summary>
        /// Applies the day or night multipliers based on the current simulation time.
        /// Called by DayNightPriceWatcher whenever day/night transitions occur.
        /// </summary>
        public static void ApplyForCurrentTime(ImprovedPublicTransport.Settings.Settings.TicketPriceCustomizerSettings settings)
        {
            if (settings == null) return;
            bool isNight = Singleton<SimulationManager>.instance.m_isNightTime;

            bool hasAfterDark    = SteamHelper.IsDLCOwned(SteamHelper.DLC.AfterDarkDLC);
            bool hasSnowfall     = SteamHelper.IsDLCOwned(SteamHelper.DLC.SnowFallDLC);
            bool hasMassTransit  = SteamHelper.IsDLCOwned(SteamHelper.DLC.InMotionDLC);
            bool hasSunsetHarbor = SteamHelper.IsDLCOwned(SteamHelper.DLC.UrbanDLC);  // Sunset Harbor = UrbanDLC
            bool hasParklife     = SteamHelper.IsDLCOwned(SteamHelper.DLC.ParksDLC);

            SetBusPrice(isNight   ? settings.BusNightMultiplier   : settings.BusMultiplier);
            SetMetroPrice(isNight ? settings.MetroNightMultiplier : settings.MetroMultiplier);
            SetTrainPrice(isNight ? settings.TrainNightMultiplier : settings.TrainMultiplier);
            SetShipPrice(isNight  ? settings.ShipNightMultiplier  : settings.ShipMultiplier);
            SetPlanePrice(isNight ? settings.PlaneNightMultiplier : settings.PlaneMultiplier);
            if (hasAfterDark)    SetTaxiPrice(isNight           ? settings.TaxiNightMultiplier           : settings.TaxiMultiplier);
            if (hasSnowfall)     SetTramPrice(isNight           ? settings.TramNightMultiplier           : settings.TramMultiplier);
            if (hasMassTransit)  SetBlimpPrice(isNight          ? settings.BlimpNightMultiplier          : settings.BlimpMultiplier);
            if (hasMassTransit)  SetFerryPrice(isNight          ? settings.FerryNightMultiplier          : settings.FerryMultiplier);
            if (hasMassTransit)  SetCableCarPrice(isNight       ? settings.CableCarNightMultiplier       : settings.CableCarMultiplier);
            if (hasMassTransit)  SetMonorailPrice(isNight       ? settings.MonorailNightMultiplier       : settings.MonorailMultiplier);
            if (hasParklife)     SetSightseeingBusPrice(isNight ? settings.SightseeingBusNightMultiplier : settings.SightseeingBusMultiplier);
            if (hasSunsetHarbor) SetIntercityBusPrice(isNight   ? settings.IntercityBusNightMultiplier   : settings.IntercityBusMultiplier);
            if (hasSunsetHarbor) SetHelicopterPrice(isNight     ? settings.HelicopterNightMultiplier     : settings.HelicopterMultiplier);
            if (hasSunsetHarbor) SetTrolleybusPrice(isNight     ? settings.TrolleybusNightMultiplier     : settings.TrolleybusMultiplier);
        }

        public static void SetSightseeingBusPrice(float multiplier)
        {
            SetPrice(multiplier, "Sightseeing Bus");
        }

        public static void SetMetroPrice(float multiplier)
        {
            SetPrice(multiplier, "Metro");
        }

        public static void SetTrainPrice(float multiplier)
        {
            SetPrice(multiplier, "Train");
        }

        public static void SetShipPrice(float multiplier)
        {
            SetPrice(multiplier, "Ship");
        }

        public static void SetPlanePrice(float multiplier)
        {
            SetPrice(multiplier, "Airplane");
        }

        public static void SetTramPrice(float multiplier)
        {
            SetPrice(multiplier, "Tram");
        }

        public static void SetBlimpPrice(float multiplier)
        {
            SetPrice(multiplier, "Blimp");
        }

        public static void SetFerryPrice(float multiplier)
        {
            SetPrice(multiplier, "Ferry");
        }

        public static void SetMonorailPrice(float multiplier)
        {
            SetPrice(multiplier, "Monorail");
        }

        public static void SetCableCarPrice(float multiplier)
        {
            SetPrice(multiplier, "CableCar");
        }

        public static void SetBusPrice(float multiplier)
        {
            SetPrice(multiplier, "Bus");
        }

        public static void SetTaxiPrice(float multiplier)
        {
            SetPrice(multiplier, "Taxi");
        }
        
        public static void SetTrolleybusPrice(float multiplier)
        {
            SetPrice(multiplier, "Trolleybus");
        }

        public static void SetHelicopterPrice(float multiplier)
        {
            SetPrice(multiplier, "Passenger Helicopter");
        }
        
        public static void SetIntercityBusPrice(float multiplier)
        {
            SetPrice(multiplier, "Intercity Bus");
        }

        // Keep track of original base prices to avoid compounding multipliers
        // (keyed by TransportInfo prefab name — see ToPrefabName).
        private static readonly System.Collections.Generic.Dictionary<string, int> s_basePrices = new System.Collections.Generic.Dictionary<string, int>();

        /// <summary>
        /// Maps ticket-tab type names to TransportInfo prefab names where they differ.
        /// Unknown names pass through unchanged.
        /// </summary>
        public static string ToPrefabName(string transportType)
        {
            switch (transportType)
            {
                case "Plane":          return "Airplane";
                case "Helicopter":     return "Passenger Helicopter";
                case "SightseeingBus": return "Sightseeing Bus";
                case "IntercityBus":   return "Intercity Bus";
                default:               return transportType;
            }
        }

        /// <summary>
        /// Base (100% multiplier) fare for a transport type, in game fare units.
        /// Falls back to the resolved prefab's current price, then to 100.
        /// </summary>
        public static int GetBasePrice(string transportType)
        {
            var key = ToPrefabName(transportType);
            if (s_basePrices.TryGetValue(key, out int basePrice) && basePrice > 0)
                return basePrice;
            // Not recorded yet (e.g. the tab was built before SetPrices ran): use the
            // resolved prefab's current price so line-less types (Taxi, CableCar) still
            // show real fares.
            if (s_cachedTransportInfos.TryGetValue(key, out var prefab) && prefab != null && prefab.m_ticketPrice > 0)
                return prefab.m_ticketPrice;
            return 100;
        }

        // Cache of resolved transport Info objects for safer simulation-thread operations.
        private static readonly System.Collections.Generic.Dictionary<string, TransportInfo> s_cachedTransportInfos = new System.Collections.Generic.Dictionary<string, TransportInfo>();

        // TransportInfo prefab names for every type the customizer supports. Types without
        // player transport lines (Taxi, CableCar, Ship, Airplane, ...) can never be found
        // by scanning TransportManager.m_lines, only through PrefabCollection.
        private static readonly string[] s_transportInfoNames =
        {
            "Bus", "Metro", "Train", "Ship", "Airplane", "Taxi", "Tram", "Blimp", "Ferry",
            "CableCar", "Monorail", "Sightseeing Bus", "Intercity Bus", "Passenger Helicopter", "Trolleybus"
        };

        /// <summary>
        /// Resolves TransportInfo prefabs for all supported types into the shared cache.
        /// PrefabCollection lookups must stay on the main thread (some mods crash on
        /// worker-thread asset deserialization), so call this from level load or UI code,
        /// never from the simulation thread. Idempotent; entries already resolved via
        /// line scans are kept.
        /// </summary>
        public static void ResolveTransportInfos()
        {
            try
            {
                foreach (var name in s_transportInfoNames)
                {
                    if (s_cachedTransportInfos.TryGetValue(name, out var cached) && cached != null)
                        continue;

                    var info = FindTransportInfoPrefab(name);
                    if (info != null)
                    {
                        s_cachedTransportInfos[name] = info;
                        if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs) IPTUtils.Log($"TicketPriceCustomizer: Resolved prefab for '{name}'.");
                    }
                }
            }
            catch (Exception e)
            {
                IPTUtils.LogError($"TicketPriceCustomizer: ResolveTransportInfos failed: {e.Message}");
            }
        }

        private static TransportInfo FindTransportInfoPrefab(string name)
        {
            var info = PrefabCollection<TransportInfo>.FindLoaded(name);
            // The cable car asset is spelled "Cable Car" in some game versions.
            if (info == null && name == "CableCar")
                info = PrefabCollection<TransportInfo>.FindLoaded("Cable Car");
            return info;
        }

        public static void ResetToVanilla()
        {
            foreach (var kvp in s_basePrices)
            {
                if (!TryGetTransportInfo(kvp.Key, out var info))
                {
                    if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs)
                        IPTUtils.LogWarning($"TicketPriceCustomizer: ResetToVanilla skipping '{kvp.Key}' - TransportInfo not found.");
                    continue;
                }

                SetInfoPrice(info, Math.Max(0, kvp.Value));

                // restore line prices for all lines with this transport type
                var transportManager = TransportManager.instance;
                if (transportManager == null) continue;

                for (ushort i = 0; i < transportManager.m_lines.m_size; i++)
                {
                    var line = transportManager.m_lines.m_buffer[i];
                    if ((line.m_flags & TransportLine.Flags.Created) == TransportLine.Flags.None || line.Info != info)
                        continue;

                    SetLinePrice(i, info, ref transportManager.m_lines.m_buffer[i], Math.Max(0, kvp.Value));
                }
            }
        }

        private static void SetPrice(float multiplier, string type)
        {
            try
            {
                // Prefer already-resolved TransportInfo objects (from existing lines) and avoid PrefabCollection lookups on simulation thread.
                if (!TryGetTransportInfo(type, out var info))
                {
                    if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs) IPTUtils.LogWarning($"TicketPriceCustomizer: TransportInfo for '{type}' not found; skipping type.");
                    return;
                }

                // Record base price if not already recorded (use original prefab value)
                var baseKey = ToPrefabName(type);
                if (!s_basePrices.TryGetValue(baseKey, out var basePrice))
                {
                    basePrice = (int)info.m_ticketPrice;
                    s_basePrices[baseKey] = basePrice;
                }

                // Calculate the full 32-bit fare. The game's per-line field is 16-bit, so the
                // AI fare getters are patched to read this value from the transport prefab.
                double rawPrice = (double)basePrice * multiplier;
                int finalPrice = rawPrice >= int.MaxValue
                    ? int.MaxValue
                    : Math.Max(0, (int)Math.Round(rawPrice, MidpointRounding.AwayFromZero));
                if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs) IPTUtils.Log($"TicketPriceCustomizer: Setting '{type}' multiplier to {multiplier:F1}x (base={basePrice}, final={finalPrice})");

                SetInfoPrice(info, finalPrice);

                int modifiedLines = 0;
                int checkedLines = 0;
                for (ushort i = 0; i < TransportManager.instance.m_lines.m_size; i++)
                {
                    var line = TransportManager.instance.m_lines.m_buffer[i];
                    checkedLines++;
                    if ((line.m_flags & TransportLine.Flags.Created) == TransportLine.Flags.None || line.Info != info)
                    {
                        continue;
                    }

                    var oldPrice = line.m_ticketPrice;
                    SetLinePrice(i, info, ref TransportManager.instance.m_lines.m_buffer[i], finalPrice);
                    if (oldPrice != TransportManager.instance.m_lines.m_buffer[i].m_ticketPrice)
                    {
                        modifiedLines++;
                    }
                }

                if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs) IPTUtils.Log($"TicketPriceCustomizer: Multiplier {multiplier:F1}x applied for '{type}': checkedLines={checkedLines}, modifiedLines={modifiedLines}");
                // Additional target logging for debugging ticket price/bus cost interactions
                IPTUtils.Log($"TicketPriceCustomizer: Applied ticket price for '{type}' (multiplier {multiplier:F1}) - checkedLines={checkedLines}, modifiedLines={modifiedLines}");
            }
            catch (Exception e)
            {
                IPTUtils.LogError($"TicketPriceCustomizer: There was an error applying multiplier {multiplier} for transport type {type}: {e.Message}");
            }
        }

        private static void SetInfoPrice(TransportInfo info, int price)
        {
            if (info.m_ticketPrice == price)
            {
               return;
            }
            if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs) IPTUtils.Log($"TicketPriceCustomizer: Transport info: {info.name}, was price: {info.m_ticketPrice}, new price: {price}");
            info.m_ticketPrice = price;
        }

        private static void SetLinePrice(ushort lineId, TransportInfo info, ref TransportLine line, int price)
        {
            // The line field is only 16-bit; the effective fare comes from the prefab via
            // the AI postfixes. Keep this clamped copy for other consumers/vanilla UI.
            ushort linePrice = (ushort)Math.Min(Math.Max(price, 0), UInt16.MaxValue);
            if ((line.m_flags & TransportLine.Flags.Created) == TransportLine.Flags.None || line.Info != info ||
                linePrice == line.m_ticketPrice)
            {
                return;
            }
            if (ImprovedPublicTransport.Util.Diagnostics.VerboseTranspileLogs) IPTUtils.Log($"TicketPriceCustomizer: Transport line id: {lineId}, #{line.m_lineNumber}, name: {Singleton<TransportManager>.instance.GetLineName(lineId)}, was price: {line.m_ticketPrice}, new price: {linePrice}");
            line.m_ticketPrice = linePrice;
        }

        private static bool TryGetTransportInfo(string transportType, out TransportInfo info)
        {
            if (s_cachedTransportInfos.TryGetValue(transportType, out info))
            {
                return info != null;
            }

            info = null;
            var transportManager = TransportManager.instance;
            if (transportManager != null)
            {
                for (ushort i = 0; i < transportManager.m_lines.m_size; i++)
                {
                    var line = transportManager.m_lines.m_buffer[i];
                    if ((line.m_flags & TransportLine.Flags.Created) == TransportLine.Flags.None || line.Info == null)
                        continue;
                    if (line.Info.name.Equals(transportType, StringComparison.OrdinalIgnoreCase))
                    {
                        info = line.Info;
                        break;
                    }
                }
            }

            // We purposely skip PrefabCollection lookups here in the simulation thread path because some external
            // mods (e.g. LoadingScreenModRevisited custom asset deserializer) can crash when called from worker threads.
            // Use line snapshot data only. Line-less types (Taxi, CableCar, ...) get their
            // prefab into this cache via ResolveTransportInfos, called on the main thread.
            s_cachedTransportInfos[transportType] = info;
            return info != null;
        }
    }
}