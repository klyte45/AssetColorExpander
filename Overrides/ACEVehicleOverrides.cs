using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.AssetColorExpander
{
    public class ACEVehicleOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            AddRedirect(typeof(VehicleAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColor", RedirectorUtils.allFlags));
            AddRedirect(typeof(VehicleAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(VehicleParked).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorParked", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerCar", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(VehicleParked).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerCarParked", RedirectorUtils.allFlags));
            AddRedirect(typeof(BusAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(CableCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerBlimpAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerFerryAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerHelicopterAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerPlaneAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerShipAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerTrainAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(TramAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(TrolleybusAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorPassengerLineVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(RocketAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorSourceBuilding", RedirectorUtils.allFlags));


            AddRedirect(typeof(VehicleManager).GetMethod("ReleaseParkedVehicle"), null, typeof(ACEVehicleOverrides).GetMethod("AfterReleaseParkedVehicle", RedirectorUtils.allFlags));
            AddRedirect(typeof(VehicleManager).GetMethod("ReleaseVehicle"), null, typeof(ACEVehicleOverrides).GetMethod("AfterReleaseVehicle", RedirectorUtils.allFlags));
        }

        public static Dictionary<string, VehicleAssetFolderRuleXml> AssetsRules => AssetColorExpanderMod.Controller?.LoadedConfiguration.m_colorConfigDataVehicles;
        public static ref Color?[] ColorCacheParked => ref AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.PARKED_VEHICLE];
        public static ref Color?[] ColorCache => ref AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.VEHICLE];
        public static ref bool[] RulesUpdatedParked => ref AssetColorExpanderMod.Controller.UpdatedRules[(int)ACEController.CacheOrder.PARKED_VEHICLE];
        public static ref bool[] RulesUpdated => ref AssetColorExpanderMod.Controller.UpdatedRules[(int)ACEController.CacheOrder.VEHICLE];

        private static bool PreGetColor_Internal(ushort vehicleId, VehicleInfo info, InfoManager.InfoMode infoMode, ref Color __result, ref Color? cacheEntry, ref bool updatedEntry, uint randomSeed, bool parked, ushort leadingVehicle)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {vehicleId}/{parked} INFO = {infoMode}");
                return true;
            }
            if (updatedEntry)
            {
                if (cacheEntry == null)
                {
                    return true;
                }
                __result = cacheEntry ?? Color.clear;
                return false;
            }

            string dataName = info?.name;
            BasicVehicleColorConfigurationXml itemData = null;
            itemData = ACEVehicleConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => parked ? x.Second.AcceptsParked(vehicleId, info) : x.Second.Accepts(vehicleId, info)).OrderBy(x => x.First).FirstOrDefault()?.Second;
            if (itemData == null && AssetsRules != null && AssetsRules.TryGetValue(dataName, out VehicleAssetFolderRuleXml itemDataAsset))
            {
                itemData = itemDataAsset;
            }

            if (itemData == null || itemData.ColoringMode == ColoringMode.SKIP)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {vehicleId} - {itemData?.ColoringMode} / not found");
                updatedEntry = true;
                cacheEntry = null;
                return true;
            }
            if (!parked && leadingVehicle != 0 && !itemData.AllowDifferentColorsOnWagons)
            {
                ushort firstVeh = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
                ref Vehicle firstVehData = ref VehicleManager.instance.m_vehicles.m_buffer[firstVeh];
                __result = firstVehData.Info.m_vehicleAI.GetColor(firstVeh, ref firstVehData, infoMode);

                updatedEntry = true;
                cacheEntry = __result;
                return false;
            }

            LogUtils.DoLog($"GETTING COLOR FOR VEHICLE: {vehicleId}");
            return ACEColorGenUtils.GetColor(randomSeed, ref __result, itemData, ref cacheEntry, ref updatedEntry);
        }
        public static bool PreGetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, ref ColorCache[vehicleID], ref RulesUpdated[vehicleID], vehicleID, false, 0);
        public static bool PreGetColorParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, data.Info, infoMode, ref __result, ref ColorCacheParked[parkedVehicleID], ref RulesUpdatedParked[parkedVehicleID], parkedVehicleID, true, 0);
        public static bool PreGetColorPassengerCar(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, ref ColorCache[vehicleID], ref RulesUpdated[vehicleID], data.m_transferSize, false, 0);
        public static bool PreGetColorPassengerCarParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, data.Info, infoMode, ref __result, ref ColorCacheParked[parkedVehicleID], ref RulesUpdatedParked[parkedVehicleID], data.m_ownerCitizen & 65535u, true, 0);
        public static bool PreGetColorPassengerLineVehicle(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (data.m_leadingVehicle != 0)
            {
                VehicleManager instance = VehicleManager.instance;
                ushort firstVehicle = data.GetFirstVehicle(vehicleID);
                if (instance.m_vehicles.m_buffer[firstVehicle].m_transportLine != 0)
                {
                    return true;
                }
            }
            else if (data.m_transportLine != 0)
            {
                return true;
            }

            return PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, ref ColorCache[vehicleID], ref RulesUpdated[vehicleID], vehicleID, false, data.m_leadingVehicle);
        }
        public static bool PreGetColorSourceBuilding(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (data.m_sourceBuilding != 0)
            {
                return true;
            }

            return PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, ref ColorCache[vehicleID], ref RulesUpdated[vehicleID], vehicleID, false, 0);
        }

        public static void AfterReleaseParkedVehicle(ushort parked)
        {
            if (AssetColorExpanderMod.Controller != null && RulesUpdatedParked != null)
            {
                RulesUpdatedParked[parked] = false;
            }
        }

        public static void AfterReleaseVehicle(ushort vehicle)
        {
            if (AssetColorExpanderMod.Controller != null && RulesUpdated != null)
            {
                RulesUpdated[vehicle] = false;
            }
        }
    }
}