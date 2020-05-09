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
            AddRedirect(typeof(PassengerCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColor", RedirectorUtils.allFlags));
            AddRedirect(typeof(PassengerCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(VehicleParked).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), GetType().GetMethod("PreGetColorParked", RedirectorUtils.allFlags));
        }

        public static Dictionary<string, VehicleAssetFolderRuleXml> AssetsRules => AssetColorExpanderMod.Controller?.m_colorConfigDataVehicles;
        public static Dictionary<ushort, BasicColorConfigurationXml> RulesCache => AssetColorExpanderMod.Controller?.m_cachedRulesVehicle;

        private static bool PreGetColor_Internal(ushort vehicleId, VehicleInfo info, InfoManager.InfoMode infoMode, ref Color __result, bool parked)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {vehicleId} INFO = {infoMode}");
                return true;
            }
            string dataName = info?.name;
            if (!RulesCache.TryGetValue(vehicleId, out BasicColorConfigurationXml itemData))
            {
                itemData = ACEVehicleConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => parked ? x.Second.AcceptsParked(vehicleId, info) : x.Second.Accepts(vehicleId, info)).OrderBy(x => x.First).FirstOrDefault()?.Second;
                if (itemData == null && AssetsRules != null && AssetsRules.TryGetValue(dataName, out VehicleAssetFolderRuleXml itemDataAsset))
                {
                    itemData = itemDataAsset;
                }
                RulesCache[vehicleId] = itemData;
            }
            if (itemData == null)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {vehicleId} - not found");
                return true;
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
                    __result = new RandomPastelColorGenerator(vehicleId, multiplier, itemData.PastelConfig).GetNext();
                    LogUtils.DoLog($"GETTING PASTEL COLOR: {__result}");
                    return false;

                case ColoringMode.LIST:
                    if (itemData.m_colorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        return true;
                    }
                    var randomizer = new Randomizer(vehicleId);

                    __result = itemData.m_colorList[randomizer.Int32((uint)itemData.m_colorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {__result}");
                    return false;
                default:
                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    return true;
            }
        }
        public static bool PreGetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(vehicleID, data.Info, infoMode, ref __result, false);
        public static bool PreGetColorParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result) => PreGetColor_Internal(parkedVehicleID, data.Info, infoMode, ref __result, false);
    }
}