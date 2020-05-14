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
using UnityEngine;

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

        public Color?[][] CachedColor { get; private set; } = new Color?[Enum.GetValues(typeof(CacheOrder)).Length][];

        public bool[][] UpdatedRules { get; private set; } = new bool[Enum.GetValues(typeof(CacheOrder)).Length][];

        public enum CacheOrder
        {
            BUILDING,
            VEHICLE,
            PARKED_VEHICLE,
            CITIZEN,
            PROP_PLACED
        }

        public enum CacheOrderSubprops
        {
            BUILDING,
            NETS
        }

        public Color?[][] CachedColorSubPropsBuildings = null;
        public bool[][] UpdatedRulesSubPropsBuildings = null;

        public Color?[][][] CachedColorSubPropsNets = null;
        public bool[][][] UpdatedRulesSubPropsNets = null;

        public Dictionary<ItemClass, List<BuildingInfo>> AllClassesBuilding { get; private set; }
        public Dictionary<ItemClass, List<VehicleInfo>> AllClassesVehicle { get; private set; }
        public Dictionary<ItemClass, List<CitizenInfo>> AllClassesCitizen { get; private set; }
        public Dictionary<ItemClass, List<PropInfo>> AllClassesProp { get; private set; }
        public Dictionary<ItemClass, List<NetInfo>> AllClassesNet { get; private set; }
        public Dictionary<Type, List<CitizenInfo>> AllAICitizen { get; private set; }

        protected override void StartActions()
        {
            ReloadFiles();
            CleanCache();
        }

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
            AllClassesProp = ((FastList<PrefabCollection<PropInfo>.PrefabData>)typeof(PrefabCollection<PropInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());
            AllClassesNet = ((FastList<PrefabCollection<NetInfo>.PrefabData>)typeof(PrefabCollection<NetInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
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
            CleanCacheProp();
        }
        public void CleanCacheVehicle()
        {
            UpdatedRules[(int)CacheOrder.VEHICLE] = new bool[VehicleManager.MAX_VEHICLE_COUNT];
            UpdatedRules[(int)CacheOrder.PARKED_VEHICLE] = new bool[VehicleManager.MAX_PARKED_COUNT];
            CachedColor[(int)CacheOrder.VEHICLE] = new Color?[VehicleManager.MAX_VEHICLE_COUNT];
            CachedColor[(int)CacheOrder.PARKED_VEHICLE] = new Color?[VehicleManager.MAX_PARKED_COUNT];
        }

        public void CleanCacheBuilding()
        {
            UpdatedRules[(int)CacheOrder.BUILDING] = new bool[BuildingManager.MAX_BUILDING_COUNT];
            CachedColor[(int)CacheOrder.BUILDING] = new Color?[BuildingManager.MAX_BUILDING_COUNT];
        }

        public void CleanCacheCitizen()
        {
            UpdatedRules[(int)CacheOrder.CITIZEN] = new bool[CitizenManager.MAX_INSTANCE_COUNT];
            CachedColor[(int)CacheOrder.CITIZEN] = new Color?[CitizenManager.MAX_INSTANCE_COUNT];
        }
        public void CleanCacheProp()
        {
            UpdatedRules[(int)CacheOrder.PROP_PLACED] = new bool[PropManager.MAX_PROP_COUNT];
            CachedColor[(int)CacheOrder.PROP_PLACED] = new Color?[PropManager.MAX_PROP_COUNT];
            UpdatedRulesSubPropsBuildings = new bool[BuildingManager.MAX_BUILDING_COUNT][];
            CachedColorSubPropsBuildings = new Color?[BuildingManager.MAX_BUILDING_COUNT][];
            UpdatedRulesSubPropsNets = new bool[NetManager.MAX_LANE_COUNT][][];
            CachedColorSubPropsNets = new Color?[NetManager.MAX_LANE_COUNT][][];
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

        #region Prefab loading

        private Dictionary<string, string> m_vehiclesLoaded, m_buildingsLoaded, m_citizensLoaded, m_propsLoaded, m_netsLoaded;

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
        public Dictionary<string, string> PropsLoaded
        {
            get {
                if (m_propsLoaded == null)
                {
                    m_propsLoaded = GetInfos<PropInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_propsLoaded;
            }
        }
        public Dictionary<string, string> NetsLoaded
        {
            get {
                if (m_netsLoaded == null)
                {
                    m_netsLoaded = GetInfos<NetInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_netsLoaded;
            }
        }


        private static string GetListName(PrefabInfo x) => (x?.name?.EndsWith("_Data") ?? false) ? $"{x?.GetLocalizedTitle()} (id: {x.name.Split('.')[0]})" : x?.name ?? "";
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

        public string[] FilterBuildingsByText(string text) => BuildingsLoaded
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
        public string[] FilterPropsByText(string text) => PropsLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        public string[] FilterNetsByText(string text) => NetsLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        #endregion

    }
}