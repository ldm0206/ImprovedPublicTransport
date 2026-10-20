using System;
using System.Reflection;
using ColossalFramework;
using Utils = ImprovedPublicTransport.Util.Utils;

namespace MileageTaxiServices
{
    /// <summary>
    /// One-shot repair for saves whose taxi income statistics were poisoned by the fare
    /// overflow fixed by <see cref="Patch_TaxiAI_UnloadPassengers"/>: each overflow dumped
    /// int.MinValue (~-2.1 billion) into the weekly income history, where it sits frozen
    /// for a whole in-game week (and is serialized into the save). Taxi income can never
    /// legitimately be negative — maintenance is booked as expenses — so negative slots
    /// are simply zeroed. Runs on the simulation thread via SimulationManager.AddAction.
    /// </summary>
    internal static class TaxiIncomeRepair
    {
        public static void SanitizeNegativeIncome()
        {
            try
            {
                var economyManager = Singleton<EconomyManager>.instance;
                if (economyManager == null)
                {
                    return;
                }

                var classIndexMethod = typeof(EconomyManager).GetMethod("ClassIndex",
                    BindingFlags.Static | BindingFlags.NonPublic, null,
                    new[] { typeof(ItemClass.Service), typeof(ItemClass.SubService), typeof(ItemClass.Level) }, null);
                var incomeField = typeof(EconomyManager).GetField("m_income",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (classIndexMethod == null || incomeField == null)
                {
                    Utils.LogWarning("MileageTaxiServices: taxi income repair skipped (EconomyManager internals not found).");
                    return;
                }

                int classIndex = (int)classIndexMethod.Invoke(null, new object[]
                {
                    ItemClass.Service.PublicTransport, ItemClass.SubService.PublicTransportTaxi, ItemClass.Level.None
                });
                if (classIndex < 0)
                {
                    return;
                }

                if (!(incomeField.GetValue(economyManager) is long[] income))
                {
                    return;
                }

                long removed = 0;
                for (int slot = 0; slot < 17; slot++)
                {
                    int i = classIndex * 17 + slot;
                    if (i < 0 || i >= income.Length)
                    {
                        break;
                    }
                    if (income[i] < 0)
                    {
                        removed += income[i];
                        income[i] = 0;
                    }
                }

                if (removed != 0)
                {
                    Utils.Log($"MileageTaxiServices: repaired corrupt taxi income history ({removed / 100.0:F2}).");
                }
            }
            catch (Exception ex)
            {
                Utils.LogError($"MileageTaxiServices: taxi income repair failed: {ex.Message}");
            }
        }
    }
}
