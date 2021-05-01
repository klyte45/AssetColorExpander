using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensions;
using System.Collections.Generic;
using UnityEngine;
using static Klyte.AssetColorExpander.ACEController;

namespace Klyte.AssetColorExpander
{
    public class ACECitizenOverrides : Redirector, IRedirectable
    {
        public void Awake()
        {
            AddRedirect(typeof(CitizenAI).GetMethod("GetColor"), typeof(ACECitizenOverrides).GetMethod("PreGetColor", RedirectorUtils.allFlags));
            AddRedirect(typeof(CitizenManager).GetMethod("ReleaseCitizenInstance"), null, typeof(ACECitizenOverrides).GetMethod("AfterReleaseCitizenInstance", RedirectorUtils.allFlags));
        }
        
        public static void AfterReleaseCitizenInstance(ushort instance) => AssetColorExpanderMod.Controller.CachedColor[(int)CacheOrder.CITIZEN][instance] = null;

        public static bool PreGetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode, ref Color __result) =>
            ACEColorGenUtils.GetColorGeneric<CitizenAssetFolderRuleXml, CitizenCityDataRuleXml, CitizenInfo>(
                ref __result,
                instanceID,
                ref AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.CITIZEN],
                infoMode, ColorParametersGetter, Accepts);

        private static void ColorParametersGetter(
            ushort id,
             out ACERulesetContainer<CitizenCityDataRuleXml> rulesGlobal,
             out Dictionary<string, CitizenAssetFolderRuleXml> assetRules,
             out CitizenInfo info,
             out Vector3 pos,
             out uint seed)
        {
            ref CitizenInstance data = ref CitizenManager.instance.m_instances.m_buffer[id];
            assetRules = AssetColorExpanderMod.Controller?.LoadedConfiguration.m_colorConfigDataCitizens;
            rulesGlobal = ACECitizenConfigRulesData.Instance.Rules;
            info = data.Info;
            pos = default;
            seed = id;
        }
        private static bool Accepts(ushort id, CitizenCityDataRuleXml x, byte district, byte park, CitizenInfo info) => x.Accepts(info);
    }
}