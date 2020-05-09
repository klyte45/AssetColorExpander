using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.AssetColorExpander
{
    public class ACEBuildingOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            AddRedirect(typeof(BuildingAI).GetMethod("GetColor"), typeof(ACEBuildingOverrides).GetMethod("PreGetColor", RedirectorUtils.allFlags));

            AddRedirect(typeof(BuildingManager).GetMethod("ReleaseBuilding"), null, typeof(ACEBuildingOverrides).GetMethod("AfterReleaseBuilding", RedirectorUtils.allFlags));
        }

        public static Dictionary<string, BuildingAssetFolderRuleXml> AssetsRules => AssetColorExpanderMod.Controller?.m_colorConfigDataBuildings;
        public static ref Color?[] ColorCache => ref AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.BUILDING];
        public static ref bool[] RulesUpdated => ref AssetColorExpanderMod.Controller.UpdatedRules[(int)ACEController.CacheOrder.BUILDING];

        public static bool PreGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {buildingID} INFO = {infoMode}");
                return true;
            }
            if (RulesUpdated[buildingID])
            {
                if (ColorCache[buildingID] == null)
                {
                    return true;
                }
                __result = ColorCache[buildingID] ?? Color.clear;
                return false;
            }

            string dataName = data.Info?.name;
            BasicColorConfigurationXml itemData;

            BuildingInfo info = data.Info;
            byte district = DistrictManager.instance.GetDistrict(data.m_position);
            byte park = DistrictManager.instance.GetPark(data.m_position);
            itemData = ACEBuildingConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Accepts(info, district, park)).OrderBy(x => x.First).FirstOrDefault()?.Second;
            if (itemData == null && AssetsRules != null && AssetsRules.TryGetValue(dataName, out BuildingAssetFolderRuleXml itemDataAsset))
            {
                itemData = itemDataAsset;
            }
            RulesUpdated[buildingID] = true;

            if (itemData == null || itemData.ColoringMode == ColoringMode.SKIP)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {buildingID} - {itemData?.ColoringMode} not found");
                ColorCache[buildingID] = null;
                RulesUpdated[buildingID] = true;
                return true;
            }
            LogUtils.DoLog($"GETTING COLOR FOR BUILDING: {buildingID}");
            return ACEColorGenUtils.GetColor(buildingID, ref __result, itemData, ref ColorCache[buildingID], ref RulesUpdated[buildingID]);
        }

        public static void AfterReleaseBuilding(ushort building) => RulesUpdated[building] = false;
    }
}