using ColossalFramework.Math;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.AssetColorExpander
{
    public static class ACEColorGenUtils
    {
        public static bool GetColor(uint seed, ref Color result, BasicColorConfigurationXml itemData, ref Color? cacheEntry, ref bool updatedEntry)
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
                    LogUtils.DoLog($"GETTING PASTEL COLOR: {result}");
                    cacheEntry = result;
                    updatedEntry = true;
                    return false;

                case ColoringMode.LIST:
                    if (itemData.m_colorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        cacheEntry = null;
                        updatedEntry = true;
                        return true;
                    }
                    var randomizer = new Randomizer(seed);

                    result = itemData.m_colorList[randomizer.Int32((uint)itemData.m_colorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {result}");
                    cacheEntry = result;
                    updatedEntry = true;
                    return false;
                default:
                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    cacheEntry = null;
                    updatedEntry = true;
                    return true;
            }
        }
    }
}