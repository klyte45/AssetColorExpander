﻿using ColossalFramework.UI;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander
{
    public class BCEController : BaseController<BuildingColorExpanderMod, BCEController>
    {
        public static UITemplateManager templateManager => UITemplateManager.instance;

        public const string DEFAULT_XML_NAME = "k45_bce_data.xml";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + "BuildingColorExpander";
        public const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralXmlConfigs";

        internal readonly Dictionary<string, AssetFolderRulesXml> m_colorConfigData = new Dictionary<string, AssetFolderRulesXml>();
        public readonly Dictionary<ushort, BasicColorConfigurationXml> m_cachedRules = new Dictionary<ushort, BasicColorConfigurationXml>();

        public Dictionary<ItemClass, List<BuildingInfo>> AllClassesBuilding { get; private set; }

        protected override void StartActions() => ReloadFiles();

        protected void Awake()
        {
            AllClassesBuilding = ((FastList<PrefabCollection<BuildingInfo>.PrefabData>)typeof(PrefabCollection<BuildingInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());
        }

        private void ReloadFiles()
        {
            m_colorConfigData.Clear();
            CleanCache();
            LoadAllBuildingConfigurations();
            if (BuildingColorExpanderMod.DebugMode)
            {
                var serializer = new XmlSerializer(typeof(AssetFolderRulesXml));
                LogUtils.DoLog($"itemCount = {m_colorConfigData.Count} \r\n" + string.Join("\r\n", m_colorConfigData.Select((x) =>
                {
                    var strWriter = new StringWriter();
                    serializer.Serialize(strWriter, x.Value);
                    string val = strWriter.ToString();
                    strWriter.Close();
                    return $"{x.Key} => [ \r\n{val}\r\n ]";
                }).ToArray()));
            }
        }

        public void CleanCache() => m_cachedRules.Clear();

        public void LoadAllBuildingConfigurations() => FileUtils.ScanPrefabsFolders<BuildingInfo>($"{DEFAULT_XML_NAME}.xml", LoadDescriptorsFromXml);

        private void LoadDescriptorsFromXml(FileStream stream, BuildingInfo info)
        {
            var serializer = new XmlSerializer(typeof(BCEConfig<AssetFolderRulesXml>));

            if (serializer.Deserialize(stream) is BCEConfig<AssetFolderRulesXml> configList)
            {
                foreach (AssetFolderRulesXml config in configList.m_dataArray)
                {
                    if (!string.IsNullOrEmpty(config.AssetName))
                    {
                        m_colorConfigData[config.AssetName] = config;
                    }
                }
            }
        }

    }
}