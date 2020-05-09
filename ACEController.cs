using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public BasicColorConfigurationXml[] CachedRulesBuilding { get; private set; } = new BasicColorConfigurationXml[BuildingManager.MAX_BUILDING_COUNT];
        public BasicVehicleColorConfigurationXml[][] CachedRulesVehicle { get; private set; } = new BasicVehicleColorConfigurationXml[][] {
            new BasicVehicleColorConfigurationXml[VehicleManager.MAX_VEHICLE_COUNT],
            new BasicVehicleColorConfigurationXml[VehicleManager.MAX_PARKED_COUNT]
        };
        public BasicColorConfigurationXml[] CachedRulesCitizen { get; private set; } = new BasicColorConfigurationXml[CitizenManager.MAX_INSTANCE_COUNT];

        public bool[] UpdatedRulesBuilding { get; private set; } = new bool[BuildingManager.MAX_BUILDING_COUNT];
        public bool[][] UpdatedRulesVehicle { get; private set; } = new bool[][] {
            new bool[VehicleManager.MAX_VEHICLE_COUNT],
            new bool[VehicleManager.MAX_PARKED_COUNT]
        };
        public bool[] UpdatedRulesCitizen { get; private set; } = new bool[CitizenManager.MAX_INSTANCE_COUNT];

        public Dictionary<ItemClass, List<BuildingInfo>> AllClassesBuilding { get; private set; }
        public Dictionary<ItemClass, List<VehicleInfo>> AllClassesVehicle { get; private set; }
        public Dictionary<ItemClass, List<CitizenInfo>> AllClassesCitizen { get; private set; }
        public Dictionary<Type, List<CitizenInfo>> AllAICitizen { get; private set; }

        protected override void StartActions() => ReloadFiles();

        protected void Awake()
        {
            AllClassesBuilding = ((FastList<PrefabCollection<BuildingInfo>.PrefabData>)typeof(PrefabCollection<BuildingInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());

            AllClassesVehicle = ((FastList<PrefabCollection<VehicleInfo>.PrefabData>)typeof(PrefabCollection<VehicleInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());

            AllClassesCitizen = ((FastList<PrefabCollection<CitizenInfo>.PrefabData>)typeof(PrefabCollection<CitizenInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());

            AllAICitizen = ((FastList<PrefabCollection<CitizenInfo>.PrefabData>)typeof(PrefabCollection<CitizenInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_citizenAI.GetType())
              .ToDictionary(x => x.First().m_citizenAI.GetType(), x => x.ToList());
            AllAICitizen[typeof(HumanAI)] = null;
            AllAICitizen[typeof(AnimalAI)] = null;
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
            CleanCacheVehicle();
            CleanCacheBuilding();
            CleanCacheCitizen();
        }
        public void CleanCacheVehicle()
        {
            UpdatedRulesVehicle = new bool[][]
            {
              new bool  [VehicleManager.MAX_VEHICLE_COUNT],
              new bool  [VehicleManager.MAX_PARKED_COUNT]
            };
        }

        public void CleanCacheBuilding() => UpdatedRulesBuilding = new bool[BuildingManager.MAX_BUILDING_COUNT];
        public void CleanCacheCitizen() => UpdatedRulesCitizen = new bool[CitizenManager.MAX_INSTANCE_COUNT];

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

        #region Prefab loading

        private Dictionary<string, string> m_vehiclesLoaded, m_buildingsLoaded, m_citizensLoaded;

        public Dictionary<string, string> VehiclesLoaded
        {
            get {
                if (m_vehiclesLoaded == null)
                {
                    m_vehiclesLoaded = GetInfos<VehicleInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_vehiclesLoaded;
            }
        }

        public Dictionary<string, string> BuildingsLoaded
        {
            get {
                if (m_buildingsLoaded == null)
                {
                    m_buildingsLoaded = GetInfos<BuildingInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_buildingsLoaded;
            }
        }

        public Dictionary<string, string> CitizensLoaded
        {
            get {
                if (m_citizensLoaded == null)
                {
                    m_citizensLoaded = GetInfos<CitizenInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_citizensLoaded;
            }
        }


        private static string GetListName(PrefabInfo x) => (x?.name?.EndsWith("_Data") ?? false) ? $"{x?.GetLocalizedTitle()}" : x?.name ?? "";
        private List<T> GetInfos<T>() where T : PrefabInfo
        {
            var list = new List<T>();
            uint num = 0u;
            while (num < (ulong)PrefabCollection<T>.LoadedCount())
            {
                T prefabInfo = PrefabCollection<T>.GetLoaded(num);
                if (prefabInfo != null)
                {
                    list.Add(prefabInfo);
                }
                num += 1u;
            }
            return list;
        }

        public string[] FilterVehiclesByText(string text) => VehiclesLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();

        public string[] FilterBuildingByText(string text) => BuildingsLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        public string[] FilterCitizensByText(string text) => CitizensLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        #endregion

    }
}