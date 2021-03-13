using ColossalFramework.Math;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.BuildingColorExpander.Extensors
{
    public class BCEColoringConfiguration : DataExtensorBase<BCEColoringConfiguration>
    {
        [XmlElement("Rules")]
        public SimpleXmlList<ColorConfigurationXml> AssetList
        {
            get => new SimpleXmlList<ColorConfigurationXml>(m_colorConfigData.Values);
            set => m_colorConfigData = value.ToDictionary(x => x.AssetName, x => x);
        }

        private Dictionary<string, ColorConfigurationXml> m_colorConfigData = new Dictionary<string, ColorConfigurationXml>();
        private static readonly Dictionary<string, ColorConfigurationXml> m_prefabsConfigData = new Dictionary<string, ColorConfigurationXml>();

        private static readonly HashSet<Type> m_loadedBuildingAIs = new HashSet<Type>();
        public static List<string> AvaliableSpecialTypes => m_loadedBuildingAIs.Select(x => x.Name).ToList();

        #region Asset List

        #endregion

        public override string SaveId => $"K45_BCE_Data";


        public static void ReloadFiles()
        {
            m_prefabsConfigData.Clear();
            LoadAllBuildingConfigurations();
            if (BuildingColorExpanderMod.DebugMode)
            {
                var serializer = new XmlSerializer(typeof(ColorConfigurationXml));
                LogUtils.DoLog($"itemCount = {m_prefabsConfigData.Count} \r\n" + string.Join("\r\n", m_prefabsConfigData.Select((x) =>
                {
                    var strWriter = new StringWriter();
                    serializer.Serialize(strWriter, x.Value);
                    string val = strWriter.ToString();
                    strWriter.Close();
                    return $"{x.Key} => [ \r\n{val}\r\n ]";
                }).ToArray()));
            }
        }

        public static bool PreGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result) => Instance.PreGetColor_internal(buildingID, ref data, infoMode, ref __result);
        private bool PreGetColor_internal(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {buildingID}; INFO = {infoMode}");
                return true;
            }
            if (!m_prefabsConfigData.TryGetValue(data.Info.name, out ColorConfigurationXml itemData)
                && !m_colorConfigData.TryGetValue(data.Info.name, out itemData)
                && !m_colorConfigData.TryGetValue(data.Info.m_buildingAI.GetType().Name, out itemData)
                && (!(data.Info.m_buildingAI is PrivateBuildingAI) || !m_colorConfigData.TryGetValue("__ZONED__", out itemData))
                && !m_colorConfigData.TryGetValue("*", out itemData))
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
                    if (itemData.ColorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        return true;
                    }
                    var randomizer = new Randomizer(buildingID);

                    __result = itemData.ColorList[randomizer.Int32((uint) itemData.ColorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {__result}");
                    return false;
                default:

                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    return true;
            }
        }

        public static void LoadAllBuildingConfigurations() => FileUtils.ScanPrefabsFolders<BuildingInfo>($"{BuildingColorExpanderMod.DEFAULT_XML_NAME}.xml", LoadDescriptorsFromXml);

        private static void LoadDescriptorsFromXml(FileStream stream, BuildingInfo prefabInfo)
        {
            m_loadedBuildingAIs.Add(prefabInfo.m_buildingAI.GetType());
            var serializer = new XmlSerializer(typeof(BceConfig));

            if (serializer.Deserialize(stream) is BceConfig configList)
            {
                foreach (ColorConfigurationXml config in configList.ConfigList)
                {
                    if (config.AssetName == null)
                    {
                        config.AssetName = prefabInfo.name;
                    }
                    if (config.AssetName == prefabInfo.name && !string.IsNullOrEmpty(config.AssetName))
                    {
                        m_prefabsConfigData[config.AssetName] = config;
                        break;
                    }
                }
            }
        }
    }
}
