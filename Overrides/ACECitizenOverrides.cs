using ColossalFramework.Math;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System.Linq;
using UnityEngine;

namespace Klyte.AssetColorExpander
{
    public class ACECitizenOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(CitizenAI).GetMethod("GetColor"), typeof(ACECitizenOverrides).GetMethod("PreGetColor", RedirectorUtils.allFlags));

        public static BasicColorConfigurationXml[] RulesCache => AssetColorExpanderMod.Controller?.CachedRulesCitizen;
        public static bool[] RulesUpdated => AssetColorExpanderMod.Controller?.UpdatedRulesCitizen;

        public static bool PreGetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode, ref Color __result)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR CITIZEN: {instanceID} INFO = {infoMode}");
                return true;
            }
            string dataName = data.Info?.name;
            ref BasicColorConfigurationXml itemData = ref RulesCache[instanceID];
            if (!RulesUpdated[instanceID])
            {
                CitizenInfo info = data.Info;
                itemData = ACECitizenConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Accepts(info)).OrderBy(x => x.First).FirstOrDefault()?.Second;

                RulesUpdated[instanceID] = true;
            }
            if (itemData == null || itemData.ColoringMode == ColoringMode.SKIP)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR CITIZEN: {instanceID} - {itemData?.ColoringMode} not found");
                return true;
            }
            LogUtils.DoLog($"GETTING COLOR FOR CITIZEN: {instanceID}");
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
                    __result = new RandomPastelColorGenerator(instanceID, multiplier, itemData.PastelConfig).GetNext();
                    LogUtils.DoLog($"GETTING PASTEL COLOR: {__result}");
                    return false;

                case ColoringMode.LIST:
                    if (itemData.m_colorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        return true;
                    }
                    var randomizer = new Randomizer(instanceID);

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