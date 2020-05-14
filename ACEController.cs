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
        public const string DEFAULT_XML_NAME_BUILDING_OLD = "k45_bce_data.xml";
        public const string DEFAULT_XML_NAME_BUILDING = "k45_ace_data_building.xml";
        public const string DEFAULT_XML_NAME_BUILDING_PROPS = "k45_ace_data_buildingProps.xml";
        public const string DEFAULT_XML_NAME_BUILDING_PROPS_GLOBAL = "k45_ace_data_buildingPropsGlobal.xml";

        public const string DEFAULT_XML_NAME_NET_PROPS = "k45_ace_data_netProps.xml";
        public const string DEFAULT_XML_NAME_NET_PROPS_GLOBAL = "k45_ace_data_netPropsGlobal.xml";

        public const string DEFAULT_XML_NAME_VEHICLE_OLD = "k45_vce_data.xml";
        public const string DEFAULT_XML_NAME_VEHICLE = "k45_ace_data_vehicle.xml";
        public const string DEFAULT_XML_NAME_CITIZEN = "k45_ace_data_citizen.xml";
        public const string DEFAULT_XML_NAME_PROP = "k45_ace_data_prop.xml";

        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + "AssetColorExpander";
        public const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralXmlConfigs";

        internal readonly Dictionary<string, BuildingAssetFolderRuleXml> m_colorConfigDataBuildings = new Dictionary<string, BuildingAssetFolderRuleXml>();
        internal readonly Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>> m_colorConfigDataBuildingsProps = new Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>>();
        internal readonly Dictionary<string, PropAssetFolderRuleXml> m_colorConfigDataBuildingsPropsGlobal = new Dictionary<string, PropAssetFolderRuleXml>();

        internal readonly Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>> m_colorConfigDataNetsProps = new Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>>();
        internal readonly Dictionary<string, PropAssetFolderRuleXml> m_colorConfigDataNetsPropsGlobal = new Dictionary<string, PropAssetFolderRuleXml>();

        internal readonly Dictionary<string, PropAssetFolderRuleXml> m_colorConfigDataProps = new Dictionary<string, PropAssetFolderRuleXml>();

        internal readonly Dictionary<string, CitizenAssetFolderRuleXml> m_colorConfigDataCitizens = new Dictionary<string, CitizenAssetFolderRuleXml>();

        internal readonly Dictionary<string, VehicleAssetFolderRuleXml> m_colorConfigDataVehicles = new Dictionary<string, VehicleAssetFolderRuleXml>();

        private FormattedReportLine[][] m_cachedLoadedReport = new FormattedReportLine[Enum.GetValues(typeof(CacheOrder)).Length][];

        internal ref FormattedReportLine[] GetLoadedReport(CacheOrder target)
        {
            switch (target)
            {
                case CacheOrder.BUILDING:
                    if (m_cachedLoadedReport[(int)CacheOrder.BUILDING] == null)
                    {
                        if (m_colorConfigDataBuildings.Count + m_colorConfigDataBuildingsProps.Count + m_colorConfigDataBuildingsPropsGlobal.Count == 0)
                        {
                            m_cachedLoadedReport[(int)CacheOrder.BUILDING] = new FormattedReportLine[0];
                        }
                        else
                        {
                            var report = new Dictionary<string, List<FormattedReportLine>>();
                            foreach (KeyValuePair<string, BuildingAssetFolderRuleXml> entry in m_colorConfigDataBuildings)
                            {
                                (report[entry.Key] ??= new List<FormattedReportLine>()).Add(new FormattedReportLine("<Self>"));
                            }
                            foreach (KeyValuePair<string, PropAssetFolderRuleXml> entry in m_colorConfigDataBuildingsPropsGlobal)
                            {
                                (report[entry.Key] ??= new List<FormattedReportLine>()).Add(new FormattedReportLine("<Global prop>"));
                            }
                            foreach (KeyValuePair<string, Dictionary<string, PropAssetFolderRuleXml>> entry in m_colorConfigDataBuildingsProps)
                            {
                                foreach (KeyValuePair<string, PropAssetFolderRuleXml> subEntry in entry.Value)
                                {
                                    (report[entry.Key] ??= new List<FormattedReportLine>()).Add(new FormattedReportLine("(Prop) " + subEntry.Key));
                                }
                            }
                            m_cachedLoadedReport[(int)CacheOrder.BUILDING] = report.SelectMany(x => new FormattedReportLine[] { new FormattedReportLine(x.Key, 0) }.Union(x.Value)).ToArray();
                        }
                    }
                    return ref m_cachedLoadedReport[(int)CacheOrder.BUILDING];
                case CacheOrder.NET:
                    if (m_cachedLoadedReport[(int)CacheOrder.NET] == null)
                    {
                        if (m_colorConfigDataNetsProps.Count + m_colorConfigDataNetsPropsGlobal.Count == 0)
                        {
                            m_cachedLoadedReport[(int)CacheOrder.NET] = new FormattedReportLine[0];
                        }
                        else
                        {
                            var report = new Dictionary<string, List<FormattedReportLine>>();
                            foreach (KeyValuePair<string, PropAssetFolderRuleXml> entry in m_colorConfigDataNetsPropsGlobal)
                            {
                                (report[entry.Key] ??= new List<FormattedReportLine>()).Add(new FormattedReportLine("<Global prop>"));
                            }
                            foreach (KeyValuePair<string, Dictionary<string, PropAssetFolderRuleXml>> entry in m_colorConfigDataNetsProps)
                            {
                                foreach (KeyValuePair<string, PropAssetFolderRuleXml> subEntry in entry.Value)
                                {
                                    (report[entry.Key] ??= new List<FormattedReportLine>()).Add(new FormattedReportLine("(Prop) " + subEntry.Key));
                                }
                            }
                            m_cachedLoadedReport[(int)CacheOrder.NET] = report.SelectMany(x => new FormattedReportLine[] { new FormattedReportLine(x.Key, 0) }.Union(x.Value)).ToArray();
                        }
                    }
                    return ref m_cachedLoadedReport[(int)CacheOrder.NET];
                case CacheOrder.PROP_PLACED:
                    if (m_cachedLoadedReport[(int)CacheOrder.PROP_PLACED] == null)
                    {
                        if (m_colorConfigDataProps.Count == 0)
                        {
                            m_cachedLoadedReport[(int)CacheOrder.PROP_PLACED] = new FormattedReportLine[0];
                        }
                        else
                        {
                            var report = new List<FormattedReportLine>();
                            foreach (KeyValuePair<string, PropAssetFolderRuleXml> entry in m_colorConfigDataProps)
                            {
                                (report).Add(new FormattedReportLine(entry.Key, 0));
                            }
                            m_cachedLoadedReport[(int)CacheOrder.PROP_PLACED] = report.ToArray();
                        }
                    }
                    return ref m_cachedLoadedReport[(int)CacheOrder.PROP_PLACED];
                case CacheOrder.VEHICLE:
                    if (m_cachedLoadedReport[(int)CacheOrder.VEHICLE] == null)
                    {
                        if (m_colorConfigDataVehicles.Count == 0)
                        {
                            m_cachedLoadedReport[(int)CacheOrder.VEHICLE] = new FormattedReportLine[0];
                        }
                        else
                        {
                            var report = new List<FormattedReportLine>();
                            foreach (KeyValuePair<string, VehicleAssetFolderRuleXml> entry in m_colorConfigDataVehicles)
                            {
                                (report).Add(new FormattedReportLine(entry.Key, 0));
                            }
                            m_cachedLoadedReport[(int)CacheOrder.VEHICLE] = report.ToArray();
                        }
                    }
                    return ref m_cachedLoadedReport[(int)CacheOrder.VEHICLE];
                case CacheOrder.CITIZEN:
                    if (m_cachedLoadedReport[(int)CacheOrder.CITIZEN] == null)
                    {
                        if (m_colorConfigDataCitizens.Count == 0)
                        {
                            m_cachedLoadedReport[(int)CacheOrder.CITIZEN] = new FormattedReportLine[0];
                        }
                        else
                        {
                            var report = new List<FormattedReportLine>();
                            foreach (KeyValuePair<string, CitizenAssetFolderRuleXml> entry in m_colorConfigDataCitizens)
                            {
                                (report).Add(new FormattedReportLine(entry.Key, 0));
                            }
                            m_cachedLoadedReport[(int)CacheOrder.CITIZEN] = report.ToArray();
                        }
                    }
                    return ref m_cachedLoadedReport[(int)CacheOrder.CITIZEN];
                default:
                    return ref nullArr;
            }
        }
        private FormattedReportLine[] nullArr = null;


        internal class FormattedReportLine
        {
            public FormattedReportLine(string text, byte level = 1)
            {
                Text = text;
                Level = level;
            }

            public byte Level { get; private set; }
            public string Text { get; private set; }

            public override string ToString()
            {
                if (Level == 0)
                {
                    return $"\t<color #FFFF00>- {Text}</color>";
                }
                else
                {
                    return $"\t\t<color #FFFFFF>- {Text}</color>";
                }
            }
        }


        public Color?[][] CachedColor { get; private set; } = new Color?[Enum.GetValues(typeof(CacheOrder)).Length][];

        public bool[][] UpdatedRules { get; private set; } = new bool[Enum.GetValues(typeof(CacheOrder)).Length][];

        public enum CacheOrder
        {
            BUILDING,
            VEHICLE,
            PARKED_VEHICLE,
            CITIZEN,
            PROP_PLACED,
            NET
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
        #region cache
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
        #endregion

        #region Load asset config files
        private void ReloadFiles()
        {
            m_colorConfigDataBuildings.Clear();
            CleanCache();
            LoadAllBuildingConfigurations();
            LoadAllVehiclesConfigurations();
            LoadAllNetConfigurations();
            LoadAllPropConfigurations();
            LoadAllCitizenConfigurations();
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
        private void LoadAllBuildingConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, BuildingInfo>>
        {
            [DEFAULT_XML_NAME_BUILDING] = LoadDescriptorsFromXml<BuildingAssetFolderRuleXml, BuildingInfo>(RegisterBuildingConfig),
            [DEFAULT_XML_NAME_BUILDING_OLD] = LoadDescriptorsFromXml<BuildingAssetFolderRuleXml, BuildingInfo>(RegisterBuildingConfig),
            [DEFAULT_XML_NAME_BUILDING_PROPS] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, BuildingInfo>(RegisterBuildingPropConfig),
            [DEFAULT_XML_NAME_BUILDING_PROPS_GLOBAL] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, BuildingInfo>(RegisterBuildingPropGlobalConfig)
        });
        private void LoadAllNetConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, NetInfo>>
        {
            [DEFAULT_XML_NAME_NET_PROPS] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, NetInfo>(RegisterNetPropConfig),
            [DEFAULT_XML_NAME_NET_PROPS_GLOBAL] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, NetInfo>(RegisterNetPropGlobalConfig)
        });
        private void LoadAllPropConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, PropInfo>>
        {
            [DEFAULT_XML_NAME_PROP] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, PropInfo>(RegisterPropConfig)
        });

        private void LoadAllCitizenConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, CitizenInfo>>
        {
            [DEFAULT_XML_NAME_CITIZEN] = LoadDescriptorsFromXml<CitizenAssetFolderRuleXml, CitizenInfo>(RegisterCitizenConfig)
        });
        private void LoadAllVehiclesConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, VehicleInfo>>
        {
            [DEFAULT_XML_NAME_VEHICLE] = LoadDescriptorsFromXml<VehicleAssetFolderRuleXml, VehicleInfo>(RegisterVehicleConfig),
            [DEFAULT_XML_NAME_VEHICLE_OLD] = LoadDescriptorsFromXml<VehicleAssetFolderRuleXml, VehicleInfo>(RegisterVehicleConfig)
        });

        private Action<FileStream, P> LoadDescriptorsFromXml<T, P>(Action<T, P> loadAction) where T : BasicColorConfigurationXml, new() where P : PrefabInfo
        {
            return (FileStream stream, P info) =>
            {
                var serializer = new XmlSerializer(typeof(ACERulesetContainer<T>));

                if (serializer.Deserialize(stream) is ACERulesetContainer<T> configList)
                {
                    foreach (T config in configList.m_dataArray)
                    {
                        loadAction(config, info);
                    }
                }
            };
        }
        public void RegisterVehicleConfig(VehicleAssetFolderRuleXml config, VehicleInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                m_colorConfigDataVehicles[info.name] = config;
            }
        }
        private void RegisterCitizenConfig(CitizenAssetFolderRuleXml config, CitizenInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                m_colorConfigDataCitizens[info.name] = config;
            }
        }
        private void RegisterPropConfig(PropAssetFolderRuleXml config, PropInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                m_colorConfigDataProps[info.name] = config;
            }
        }
        private void RegisterBuildingConfig(BuildingAssetFolderRuleXml config, BuildingInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                m_colorConfigDataBuildings[info.name] = config;
            }
        }
        private void RegisterBuildingPropConfig(PropAssetFolderRuleXml config, BuildingInfo info)
        {
            if (config.AssetName != null)
            {
                config.BuildingName = info.name;
                if (m_colorConfigDataBuildingsProps[info.name] == null)
                {
                    m_colorConfigDataBuildingsProps[info.name] = new Dictionary<string, PropAssetFolderRuleXml>();
                }
                m_colorConfigDataBuildingsProps[info.name][config.AssetName] = config;
            }
        }
        private void RegisterBuildingPropGlobalConfig(PropAssetFolderRuleXml config, BuildingInfo info)
        {
            if (config.AssetName == null)
            {
                config.BuildingName = info.name;
                m_colorConfigDataBuildingsPropsGlobal[info.name] = config;
            }
        }


        private void RegisterNetPropConfig(PropAssetFolderRuleXml config, NetInfo info)
        {
            if (config.AssetName != null)
            {
                config.NetName = info.name;
                if (m_colorConfigDataNetsProps[info.name] == null)
                {
                    m_colorConfigDataNetsProps[info.name] = new Dictionary<string, PropAssetFolderRuleXml>();
                }
                m_colorConfigDataNetsProps[info.name][config.AssetName] = config;
            }
        }
        private void RegisterNetPropGlobalConfig(PropAssetFolderRuleXml config, NetInfo info)
        {
            if (config.AssetName == null)
            {
                config.NetName = info.name;
                m_colorConfigDataNetsPropsGlobal[info.name] = config;
            }
        }
        #endregion

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