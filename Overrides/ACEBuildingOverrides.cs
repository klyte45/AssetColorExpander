using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensions;
using System.Collections.Generic;
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

        public static bool PreGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result) =>
            ACEColorGenUtils.GetColorGeneric<BuildingAssetFolderRuleXml, BuildingCityDataRuleXml, BuildingInfo>(
                ref __result,
                buildingID,
                ref AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.BUILDING],
                infoMode, ColorParametersGetter, Accepts, (x, y) => buildingID);

        private static void ColorParametersGetter(
            ushort buildingID,
             out ACERulesetContainer<BuildingCityDataRuleXml> rulesGlobal,
             out Dictionary<string, BuildingAssetFolderRuleXml> assetRules,
             out BuildingInfo info,
             out Vector3 pos)
        {
            ref Building data = ref BuildingManager.instance.m_buildings.m_buffer[buildingID];
            assetRules = AssetColorExpanderMod.Controller?.LoadedConfiguration.m_colorConfigDataBuildings;
            rulesGlobal = ACEBuildingConfigRulesData.Instance.Rules;
            info = data.Info;
            pos = data.m_position;
        }
        private static bool Accepts(ushort id, BuildingCityDataRuleXml x, byte district, byte park, BuildingInfo info) => x.Accepts(info, district, park);

        public static void AfterReleaseBuilding(ushort building) => AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.BUILDING][building] = null;
    }
}