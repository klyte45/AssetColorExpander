using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Linq;
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

        public static ref Color?[] ColorCache => ref AssetColorExpanderMod.Controller.CachedColor[(int)CacheOrder.CITIZEN];
        public static ref bool[] RulesUpdated => ref AssetColorExpanderMod.Controller.UpdatedRules[(int)CacheOrder.CITIZEN];

        public static bool PreGetColor(ushort instanceID, ref CitizenInstance data, InfoManager.InfoMode infoMode, ref Color __result)
        {

            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR CITIZEN: {instanceID} INFO = {infoMode}");
                return true;
            }
            if (RulesUpdated[instanceID])
            {
                if (ColorCache[instanceID] == null)
                {
                    return true;
                }
                __result = ColorCache[instanceID] ?? Color.clear;
                return false;
            }

            string dataName = data.Info?.name;
            BasicColorConfigurationXml itemData = null;
            CitizenInfo info = data.Info;
            itemData = ACECitizenConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Accepts(info)).OrderBy(x => x.First).FirstOrDefault()?.Second;
            if (itemData == null || itemData.ColoringMode == ColoringMode.SKIP)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR CITIZEN: {instanceID} - {itemData?.ColoringMode} not found");
                ColorCache[instanceID] = null;
                RulesUpdated[instanceID] = true;
                return true;
            }
            LogUtils.DoLog($"GETTING COLOR FOR CITIZEN: {instanceID}");
            return ACEColorGenUtils.GetColor(instanceID, ref __result, itemData, ref ColorCache[instanceID], ref RulesUpdated[instanceID]);
        }

        public static void AfterReleaseCitizenInstance(ushort instance)
        {
            if (AssetColorExpanderMod.Controller != null && RulesUpdated != null)
            {
                RulesUpdated[instance] = false;
            }
        }
    }
}