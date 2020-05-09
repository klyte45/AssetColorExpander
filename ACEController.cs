using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander
{
    public class ACEController : BaseController<AssetColorExpanderMod, ACEController>
    {
        public const string DEFAULT_XML_NAME_BUILDING = "k45_bce_data.xml";
        public const string DEFAULT_XML_NAME_VEHICLE = "k45_vce_data.xml";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + "AssetColorExpander";
        public const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralXmlConfigs";

        internal readonly Dictionary<string, BuildingAssetFolderRuleXml> m_colorConfigDataBuildings = new Dictionary<string, BuildingAssetFolderRuleXml>();
        internal readonly Dictionary<string, VehicleAssetFolderRuleXml> m_colorConfigDataVehicles = new Dictionary<string, VehicleAssetFolderRuleXml>();
        public readonly Dictionary<ushort, BasicColorConfigurationXml> m_cachedRulesBuilding = new Dictionary<ushort, BasicColorConfigurationXml>();
        public readonly Dictionary<ushort, BasicColorConfigurationXml> m_cachedRulesVehicle = new Dictionary<ushort, BasicColorConfigurationXml>();

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
            m_colorConfigDataBuildings.Clear();
            CleanCache();
            LoadAllBuildingConfigurations();
            LoadAllVehiclesConfigurations();
            if (AssetColorExpanderMod.DebugMode)
            {
                var serializer = new XmlSerializer(typeof(BuildingAssetFolderRuleXml));
                LogUtils.DoLog($"[Building] itemCount = {m_colorConfigDataBuildings.Count} \r\n" + string.Join("\r\n", m_colorConfigDataBuildings.Select((x) =>
                {
                    var strWriter = new StringWriter();
                    serializer.Serialize(strWriter, x.Value);
                    string val = strWriter.ToString();
                    strWriter.Close();
                    return $"{x.Key} => [ \r\n{val}\r\n ]";
                }).ToArray()));

                serializer = new XmlSerializer(typeof(VehicleAssetFolderRuleXml));
                LogUtils.DoLog($"[Vehicle] itemCount = {m_colorConfigDataVehicles.Count} \r\n" + string.Join("\r\n", m_colorConfigDataVehicles.Select((x) =>
                {
                    var strWriter = new StringWriter();
                    serializer.Serialize(strWriter, x.Value);
                    string val = strWriter.ToString();
                    strWriter.Close();
                    return $"{x.Key} => [ \r\n{val}\r\n ]";
                }).ToArray()));
            }
        }
        public void LoadAllVehiclesConfigurations() => Commons.Utils.FileUtils.ScanPrefabsFolders<VehicleInfo>($"{DEFAULT_XML_NAME_VEHICLE}.xml", LoadDescriptorsFromXml);

        private void LoadDescriptorsFromXml(FileStream stream, VehicleInfo info)
        {

            var serializer = new XmlSerializer(typeof(ACERulesetContainer<VehicleAssetFolderRuleXml>));

            if (serializer.Deserialize(stream) is ACERulesetContainer<VehicleAssetFolderRuleXml> configList)
            {
                foreach (VehicleAssetFolderRuleXml config in configList.m_dataArray)
                {
                    if (!string.IsNullOrEmpty(config.AssetName))
                    {
                        m_colorConfigDataVehicles[config.AssetName] = config;
                    }
                }
            }
        }

        public void CleanCache()
        {
            m_cachedRulesBuilding.Clear();
            m_cachedRulesVehicle.Clear();
        }

        public void LoadAllBuildingConfigurations() => FileUtils.ScanPrefabsFolders<BuildingInfo>($"{DEFAULT_XML_NAME_BUILDING}.xml", LoadDescriptorsFromXml);

        private void LoadDescriptorsFromXml(FileStream stream, BuildingInfo info)
        {
            var serializer = new XmlSerializer(typeof(ACERulesetContainer<BuildingAssetFolderRuleXml>));

            if (serializer.Deserialize(stream) is ACERulesetContainer<BuildingAssetFolderRuleXml> configList)
            {
                foreach (BuildingAssetFolderRuleXml config in configList.m_dataArray)
                {
                    if (!string.IsNullOrEmpty(config.AssetName))
                    {
                        m_colorConfigDataBuildings[config.AssetName] = config;
                    }
                }
            }
        }

    }
}