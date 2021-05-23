using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensions;
using System;
using System.Collections.Generic;
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


        public static bool PreGetColor_Internal(ushort vehicleId, InfoManager.InfoMode infoMode, ref Color __result, uint randomSeed, bool parked) =>
            ACEColorGenUtils.GetColorGeneric(
                ref __result,
                vehicleId,
                ref AssetColorExpanderMod.Controller.CachedColor[parked ? (int)ACEController.CacheOrder.PARKED_VEHICLE : (int)ACEController.CacheOrder.VEHICLE],
                infoMode, ColorParametersGetter(parked), parked ? AcceptsParked : Accepts(), (x, y) => GetSeed(parked, x, y));
        private static uint GetSeed(bool isParked, int arg1, BasicColorConfigurationXml arg2)
        {
            if (isParked || (arg2 is VehicleCityDataRuleXml vc && vc.AllowDifferentColorsOnWagons) || (arg2 is VehicleAssetFolderRuleXml vf && vf.AllowDifferentColorsOnWagons))
            {
                return (uint)arg1;
            }
            else
            {
                ref Vehicle data = ref VehicleManager.instance.m_vehicles.m_buffer[arg1];
                return data.m_leadingVehicle > 0 ?
                VehicleManager.instance.m_vehicles.m_buffer[arg1].GetFirstVehicle((ushort)arg1)
                : (uint)arg1;
            }
        }
        private static ColorParametersGetter<VehicleAssetFolderRuleXml, VehicleCityDataRuleXml, VehicleInfo> ColorParametersGetter(bool isParked) =>
             (
             ushort vehicleId,
             out ACERulesetContainer<VehicleCityDataRuleXml> rulesGlobal,
             out Dictionary<string, VehicleAssetFolderRuleXml> assetRules,
             out VehicleInfo info,
             out Vector3 pos) =>
        {
            if (isParked)
            {
                ref VehicleParked data = ref VehicleManager.instance.m_parkedVehicles.m_buffer[vehicleId];
                info = data.Info;
                pos = default;
            }
            else
            {
                ref Vehicle data = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                info = data.Info;
                pos = data.m_sourceBuilding != 0 ? BuildingManager.instance.m_buildings.m_buffer[data.m_sourceBuilding].m_position : default;
            }
            assetRules = AssetColorExpanderMod.Controller?.LoadedConfiguration.m_colorConfigDataVehicles;
            rulesGlobal = ACEVehicleConfigRulesData.Instance.Rules;
        };
        private static RuleValidator<VehicleCityDataRuleXml, VehicleInfo> Accepts() => (id, x, district, park, info) =>
        {
            ref Vehicle data = ref VehicleManager.instance.m_vehicles.m_buffer[id];
            return x.Accepts(id, info, data.m_transportLine);
        };
        private static RuleValidator<VehicleCityDataRuleXml, VehicleInfo> AcceptsParked = (ushort id, VehicleCityDataRuleXml x, byte district, byte park, VehicleInfo info) => x.AcceptsParked(id, info);



        public static bool PreGetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, infoMode, ref __result, vehicleID, false);
        public static bool PreGetColorParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, infoMode, ref __result, parkedVehicleID, true);
        public static bool PreGetColorPassengerCar(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, infoMode, ref __result, data.m_transferSize, false);
        public static bool PreGetColorPassengerCarParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, infoMode, ref __result, data.m_ownerCitizen & 65535u, true);
        public static bool PreGetColorPassengerLineVehicle(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (ColorCache[vehicleID] is Color clr)
            {
                if (clr.a < 1)
                {
                    return true;
                }
                else
                {
                    __result = clr;
                    return false;
                }
            }
            if (data.m_leadingVehicle != 0)
            {
                VehicleManager instance = VehicleManager.instance;
                ushort firstVehicle = data.GetFirstVehicle(vehicleID);
                if (instance.m_vehicles.m_buffer[firstVehicle].m_transportLine != 0)
                {
                    return true;
                }
            }

            return PreGetColor_Internal(vehicleID, infoMode, ref __result, data.m_leadingVehicle != 0 ? vehicleID : data.m_leadingVehicle, false);
        }
        public static bool PreGetColorSourceBuilding(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (ColorCache[vehicleID] is Color clr)
            {
                if (clr.a < 1)
                {
                    return true;
                }
                else
                {
                    __result = clr;
                    return false;
                }
            }
            return data.m_sourceBuilding != 0
                ? true
                : PreGetColor_Internal(vehicleID, infoMode, ref __result, vehicleID, false);
        }

        public static void AfterReleaseParkedVehicle(ushort parked) => ColorCacheParked[parked] = null;

        public static void AfterReleaseVehicle(ushort vehicle) => ColorCache[vehicle] = null;

    }
}