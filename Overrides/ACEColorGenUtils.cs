using ColossalFramework.Math;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.AssetColorExpander
{
    public delegate void ColorParametersGetter<F, C, I>(
        ushort id,
        out ACERulesetContainer<C> rulesGlobal,
           out Dictionary<string, F> assetRules,
           out I info,
           out Vector3 pos)
        where F : BasicColorConfigurationXml, new()
        where C : BasicColorConfigurationXml, new()
        where I : PrefabInfo;

    public delegate bool RuleValidator<C, I>(ushort id, C rule, byte district, byte park, I info)
        where C : BasicColorConfigurationXml, new()
        where I : PrefabInfo;

    public static class ACEColorGenUtils
    {
        public static bool GetColorGeneric<F, C, I>(
            ref Color __result,
            ushort id,
            ref Color?[] colorCacheArray,
            InfoManager.InfoMode infoMode,
            ColorParametersGetter<F, C, I> getter,
            RuleValidator<C, I> ruleValidator,
            Func<int, BasicColorConfigurationXml, uint> getSeed)
            where F : BasicColorConfigurationXml, new()
            where C : BasicColorConfigurationXml, new()
            where I : PrefabInfo
        {
            if (infoMode != InfoManager.InfoMode.None)
            {
                if (CommonProperties.DebugMode)
                {
                    LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {id} INFO = {infoMode}");
                }

                return true;
            }
            ref Color? resultColor = ref colorCacheArray[id];
            if (resultColor is Color res)
            {
                if (res.a < 1)
                {
                    return true;
                }
                __result = res;
                return false;
            }

            getter(id, out ACERulesetContainer<C> rulesGlobal, out Dictionary<string, F> assetRules, out I info, out Vector3 pos);
            string dataName = info?.name;
            BasicColorConfigurationXml itemData;

            byte district = DistrictManager.instance.GetDistrict(pos);
            byte park = DistrictManager.instance.GetPark(pos);
            itemData = rulesGlobal.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => ruleValidator(id, x.Second, district, park, info)).OrderBy(x => x.First).FirstOrDefault()?.Second;
            if (itemData == null && assetRules != null && assetRules.TryGetValue(dataName, out F itemDataAsset))
            {
                itemData = itemDataAsset;
            }

            if (itemData == null || itemData.ColoringMode == ColoringMode.SKIP)
            {
                if (CommonProperties.DebugMode)
                {
                    LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {id} - {itemData?.ColoringMode} not found");
                }

                colorCacheArray[id] = default(Color);
                return true;
            }
            if (CommonProperties.DebugMode)
            {
                LogUtils.DoLog($"GETTING COLOR FOR BUILDING: {id}");
            }

            return GetColor(getSeed(id, itemData), ref __result, itemData, ref resultColor, district);
        }
        private static bool GetColor(uint seed, ref Color result, BasicColorConfigurationXml itemData, ref Color? cacheEntry, byte districtId)
        {
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
                    result = new RandomPastelColorGenerator(seed, multiplier, itemData.PastelConfig).GetNext();
                    if (CommonProperties.DebugMode)
                    {
                        LogUtils.DoLog($"GETTING PASTEL COLOR: {result}");
                    }

                    cacheEntry = result;
                    return false;

                case ColoringMode.LIST:
                    if (itemData.m_colorList.Count == 0)
                    {
                        if (CommonProperties.DebugMode)
                        {
                            LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        }

                        cacheEntry = default(Color);
                        return true;
                    }
                    var randomizer = new Randomizer(seed);

                    result = itemData.m_colorList[randomizer.Int32((uint)itemData.m_colorList.Count)];
                    if (CommonProperties.DebugMode)
                    {
                        LogUtils.DoLog($"GETTING LIST COLOR: {result}");
                    }

                    cacheEntry = result;
                    return false;
                case ColoringMode.DISTRICT:
                    cacheEntry = AssetColorExpanderMod.Controller.ConnectorADR.GetDistrictColor(districtId);
                    return false;
                default:
                    if (CommonProperties.DebugMode)
                    {
                        LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    }

                    cacheEntry = default(Color);
                    return true;
            }
        }
    }
}