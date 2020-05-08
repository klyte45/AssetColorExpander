using ColossalFramework.Math;
using Klyte.BuildingColorExpander.Data;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.BuildingColorExpander
{
    public class BCEOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(BuildingAI).GetMethod("GetColor"), typeof(BCEOverrides).GetMethod("PreGetColor", RedirectorUtils.allFlags));

        public static Dictionary<string, AssetFolderRulesXml> AssetsRules => BuildingColorExpanderMod.Controller?.m_colorConfigData;
        public static Dictionary<ushort, BasicColorConfigurationXml> RulesCache => BuildingColorExpanderMod.Controller?.m_cachedRules;

        public static bool PreGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {buildingID} INFO = {infoMode}");
                return true;
            }
            string dataName = data.Info?.name;
            if (!RulesCache.TryGetValue(buildingID, out BasicColorConfigurationXml itemData))
            {
                BuildingInfo info = data.Info;
                byte district = DistrictManager.instance.GetDistrict(data.m_position);
                byte park = DistrictManager.instance.GetPark(data.m_position);
                itemData = BCEConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Accepts(info, district, park)).OrderBy(x => x.First).FirstOrDefault()?.Second;
                if (itemData == null && AssetsRules != null && AssetsRules.TryGetValue(dataName, out AssetFolderRulesXml itemDataAsset))
                {
                    itemData = itemDataAsset;
                }
                RulesCache[buildingID] = itemData;
            }
            if (itemData == null)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {buildingID} - not found");
                return true;
            }
            LogUtils.DoLog($"GETTING COLOR FOR BUILDING: {buildingID}");
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
                    __result = new RandomPastelColorGenerator(buildingID, multiplier, itemData.PastelConfig).GetNext();
                    LogUtils.DoLog($"GETTING PASTEL COLOR: {__result}");
                    return false;

                case ColoringMode.LIST:
                    if (itemData.m_colorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        return true;
                    }
                    var randomizer = new Randomizer(buildingID);

                    __result = itemData.m_colorList[randomizer.Int32((uint)itemData.m_colorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {__result}");
                    return false;
                default:
                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    return true;
            }
        }
    }
}