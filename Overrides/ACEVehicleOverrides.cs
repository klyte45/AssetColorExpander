using ColossalFramework.Math;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
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
        }

        public static Dictionary<string, VehicleAssetFolderRuleXml> AssetsRules => AssetColorExpanderMod.Controller?.m_colorConfigDataVehicles;
        public static BasicVehicleColorConfigurationXml[][] RulesCache => AssetColorExpanderMod.Controller?.CachedRulesVehicle;
        public static bool[][] RulesUpdated => AssetColorExpanderMod.Controller?.UpdatedRulesVehicle;

        private static bool PreGetColor_Internal(ushort vehicleId, VehicleInfo info, InfoManager.InfoMode infoMode, ref Color __result, uint randomSeed, bool parked, ushort leadingVehicle)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {vehicleId} INFO = {infoMode}");
                return true;
            }
            string dataName = info?.name;
            int idx = parked ? 0 : 1;
            ref BasicVehicleColorConfigurationXml itemData = ref RulesCache[idx][vehicleId];
            if (!RulesUpdated[idx][vehicleId])
            {
                itemData = ACEVehicleConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => parked ? x.Second.AcceptsParked(vehicleId, info) : x.Second.Accepts(vehicleId, info)).OrderBy(x => x.First).FirstOrDefault()?.Second;
                if (itemData == null && AssetsRules != null && AssetsRules.TryGetValue(dataName, out VehicleAssetFolderRuleXml itemDataAsset))
                {
                    itemData = itemDataAsset;
                }
                RulesUpdated[idx][vehicleId] = true;
            }
            if (itemData == null)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {vehicleId} - not found");
                return true;
            }
            if (!parked && leadingVehicle != 0 && !itemData.AllowDifferentColorsOnWagons)
            {
                ushort firstVeh = VehicleManager.instance.m_vehicles.m_buffer[vehicleId].GetFirstVehicle(vehicleId);
                ref Vehicle firstVehData = ref VehicleManager.instance.m_vehicles.m_buffer[firstVeh];
                __result = firstVehData.Info.m_vehicleAI.GetColor(firstVeh, ref firstVehData, infoMode);
                return false;
            }

            LogUtils.DoLog($"GETTING COLOR FOR VEHICLE: {vehicleId}");
            float multiplier;
            switch (itemData.ColoringMode)
            {
                case ColoringMode.PASTEL_FULL_VIVID:
                    multiplier = 1.3f;
                    goto CASE_ORIG;
                case ColoringMode.PASTEL_HIGHER_SATURATION:
                    multiplier = 1.1f;
                    goto CASE_ORIG;
                case ColoringMode.PASTEL_ORIG:
                    multiplier = 1f;
                CASE_ORIG:
                    __result = new RandomPastelColorGenerator(randomSeed, multiplier, itemData.PastelConfig).GetNext();
                    LogUtils.DoLog($"GETTING PASTEL COLOR: {__result}");
                    return false;

                case ColoringMode.LIST:
                    if (itemData.m_colorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        return true;
                    }
                    var randomizer = new Randomizer(randomSeed);

                    __result = itemData.m_colorList[randomizer.Int32((uint)itemData.m_colorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {__result}");
                    return false;
                default:
                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    return true;
            }
        }
        public static bool PreGetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, vehicleID, false, 0);
        public static bool PreGetColorParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, data.Info, infoMode, ref __result, parkedVehicleID, false, 0);
        public static bool PreGetColorPassengerCar(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, data.m_transferSize, false, 0);
        public static bool PreGetColorPassengerCarParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, data.Info, infoMode, ref __result, data.m_ownerCitizen & 65535u, false, 0);
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

            return PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, vehicleID, false, data.m_leadingVehicle);
        }
        public static bool PreGetColorSourceBuilding(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (data.m_sourceBuilding != 0)
            {
                return true;
            }

            return PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, vehicleID, false, 0);
        }
    }
}