using ColossalFramework.Globalization;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using static Klyte.AssetColorExpander.ACEController;

namespace Klyte.AssetColorExpander
{
    public class ACELoadedDataContainer
    {

        public const string DEFAULT_XML_NAME_VEHICLE_OLD = "k45_vce_data.xml";
        public const string DEFAULT_XML_NAME_BUILDING_OLD = "k45_bce_data.xml";

        public const string DEFAULT_XML_NAME_BUILDING = "k45_ace_data_building.xml";
        public const string DEFAULT_XML_NAME_BUILDING_PROPS = "k45_ace_data_buildingProps.xml";
        public const string DEFAULT_XML_NAME_BUILDING_PROPS_GLOBAL = "k45_ace_data_buildingPropsGlobal.xml";
        public const string DEFAULT_XML_NAME_BUILDING_VEHICLES = "k45_ace_data_buildingVehicles.xml";
        public const string DEFAULT_XML_NAME_BUILDING_VEHICLES_GLOBAL = "k45_ace_data_buildingVehiclesGlobal.xml";

        public const string DEFAULT_XML_NAME_NET_PROPS = "k45_ace_data_netProps.xml";
        public const string DEFAULT_XML_NAME_NET_PROPS_GLOBAL = "k45_ace_data_netPropsGlobal.xml";

        public const string DEFAULT_XML_NAME_VEHICLE = "k45_ace_data_vehicle.xml";
        public const string DEFAULT_XML_NAME_CITIZEN = "k45_ace_data_citizen.xml";
        public const string DEFAULT_XML_NAME_PROP = "k45_ace_data_prop.xml";



        internal readonly Dictionary<string, BuildingAssetFolderRuleXml> m_colorConfigDataBuildings = new Dictionary<string, BuildingAssetFolderRuleXml>();
        internal readonly Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>> m_colorConfigDataBuildingsProps = new Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>>();
        internal readonly Dictionary<string, PropAssetFolderRuleXml> m_colorConfigDataBuildingsPropsGlobal = new Dictionary<string, PropAssetFolderRuleXml>();
        internal readonly Dictionary<string, VehicleAssetFolderRuleXml> m_colorConfigDataBuildingsVehiclesGlobal = new Dictionary<string, VehicleAssetFolderRuleXml>();

        internal readonly Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>> m_colorConfigDataNetsProps = new Dictionary<string, Dictionary<string, PropAssetFolderRuleXml>>();
        internal readonly Dictionary<string, PropAssetFolderRuleXml> m_colorConfigDataNetsPropsGlobal = new Dictionary<string, PropAssetFolderRuleXml>();

        internal readonly Dictionary<string, PropAssetFolderRuleXml> m_colorConfigDataProps = new Dictionary<string, PropAssetFolderRuleXml>();

        internal readonly Dictionary<string, CitizenAssetFolderRuleXml> m_colorConfigDataCitizens = new Dictionary<string, CitizenAssetFolderRuleXml>();

        internal readonly Dictionary<string, VehicleAssetFolderRuleXml> m_colorConfigDataVehicles = new Dictionary<string, VehicleAssetFolderRuleXml>();


        #region Load asset config files
        internal void ReloadFiles()
        {
            m_cachedLoadedReport = new FormattedReportLine[Enum.GetValues(typeof(CacheOrder)).Length][];
            m_colorConfigDataBuildings.Clear();
            m_colorConfigDataBuildingsProps.Clear();
            m_colorConfigDataBuildingsPropsGlobal.Clear();
            m_colorConfigDataBuildingsVehiclesGlobal.Clear();
            m_colorConfigDataNetsProps.Clear();
            m_colorConfigDataNetsPropsGlobal.Clear();
            m_colorConfigDataProps.Clear();
            m_colorConfigDataCitizens.Clear();
            m_colorConfigDataVehicles.Clear();
            LoadAllBuildingConfigurations();
            LoadAllVehiclesConfigurations();
            LoadAllNetConfigurations();
            LoadAllPropConfigurations();
            LoadAllCitizenConfigurations();
            LoadAllLocalLibs();


            AssetColorExpanderMod.Instance.m_building.text = Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_BUILDINGS") + $" ({m_colorConfigDataBuildings.Count + m_colorConfigDataBuildingsProps.Count + m_colorConfigDataBuildingsVehiclesGlobal.Count + m_colorConfigDataBuildingsPropsGlobal.Count})";
            AssetColorExpanderMod.Instance.m_citizen.text = Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_CITIZENS") + $" ({m_colorConfigDataCitizens.Count})";
            AssetColorExpanderMod.Instance.m_net.text = Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_NETWORKS") + $" ({m_colorConfigDataNetsProps.Count + m_colorConfigDataNetsPropsGlobal.Count})";
            AssetColorExpanderMod.Instance.m_prop.text = Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_PROPS") + $" ({m_colorConfigDataProps.Count})";
            AssetColorExpanderMod.Instance.m_vehicle.text = Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_VEHICLES") + $" ({m_colorConfigDataVehicles.Count})";
        }
        private void LoadAllLocalLibs()
        {
            LoadLocalConfiguration(m_colorConfigDataBuildings, DEFAULT_XML_NAME_BUILDING);
            LoadLocalConfiguration(m_colorConfigDataVehicles, DEFAULT_XML_NAME_VEHICLE);
            LoadLocalConfiguration(m_colorConfigDataCitizens, DEFAULT_XML_NAME_CITIZEN);
            LoadLocalConfiguration(m_colorConfigDataProps, DEFAULT_XML_NAME_PROP);
            LoadLocalConfiguration(m_colorConfigDataBuildingsPropsGlobal, DEFAULT_XML_NAME_BUILDING_PROPS_GLOBAL);
            LoadLocalConfiguration(m_colorConfigDataNetsPropsGlobal, DEFAULT_XML_NAME_NET_PROPS_GLOBAL);
            LoadLocalConfiguration(m_colorConfigDataBuildingsVehiclesGlobal, DEFAULT_XML_NAME_BUILDING_VEHICLES_GLOBAL);
        }

        private void LoadLocalConfiguration<T>(Dictionary<string, T> target, string file) where T : BasicColorConfigurationXml, IAssetNameable, IRuleCacheSource, new()
        {
            string path = Path.Combine(ACEController.FOLDER_PATH_GENERAL_CONFIG, file);
            if (File.Exists(path))
            {
                ACERulesetContainer<T> container = XmlUtils.DefaultXmlDeserialize<ACERulesetContainer<T>>(File.ReadAllText(path));
                foreach (T item in container.m_dataArray)
                {
                    if (item.AssetName != null)
                    {
                        item.Source = RuleSource.LOCAL;
                        target[item.AssetName] = item;
                    }
                }
            }
        }

        private void LoadAllBuildingConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, BuildingInfo>>
        {
            [DEFAULT_XML_NAME_BUILDING] = LoadDescriptorsFromXml<BuildingAssetFolderRuleXml, BuildingInfo>(RegisterBuildingConfig),
            [DEFAULT_XML_NAME_BUILDING_OLD] = LoadDescriptorsFromXml<BuildingAssetFolderRuleXml, BuildingInfo>(RegisterBuildingConfig),
            [DEFAULT_XML_NAME_BUILDING_VEHICLES] = LoadDescriptorsFromXml<VehicleAssetFolderRuleXml, BuildingInfo>(RegisterBuildingVehicleConfig),
            [DEFAULT_XML_NAME_BUILDING_PROPS] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, BuildingInfo>(RegisterBuildingPropConfig),
        });
        private void LoadAllNetConfigurations() => FileUtils.ScanPrefabsFolders(new Dictionary<string, Action<FileStream, NetInfo>>
        {
            [DEFAULT_XML_NAME_NET_PROPS] = LoadDescriptorsFromXml<PropAssetFolderRuleXml, NetInfo>(RegisterNetPropConfig),
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
            [DEFAULT_XML_NAME_VEHICLE_OLD] = LoadDescriptorsFromXml<VehicleAssetFolderRuleXml, VehicleInfo>(RegisterVehicleConfig),
        });

        private Action<FileStream, P> LoadDescriptorsFromXml<T, P>(Action<T, P> loadAction) where T : BasicColorConfigurationXml, new() where P : PrefabInfo
        {
            return (FileStream stream, P info) =>
            {
                var serializer = new XmlSerializer(typeof(ACERulesetContainer<T>));

                try
                {
                    if (serializer.Deserialize(stream) is ACERulesetContainer<T> configList)
                    {
                        foreach (T config in configList.m_dataArray)
                        {
                            loadAction(config, info);

                        }
                    }
                }
                catch (Exception e)
                {
                    LogUtils.DoWarnLog($"Exception while loading file xml: {e.Message}\n{e.StackTrace}");
                }
            };
        }
        public void RegisterVehicleConfig(VehicleAssetFolderRuleXml config, VehicleInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                config.Source = RuleSource.ASSET;
                m_colorConfigDataVehicles[info.name] = config;
            }
        }
        private void RegisterCitizenConfig(CitizenAssetFolderRuleXml config, CitizenInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                config.Source = RuleSource.ASSET;
                m_colorConfigDataCitizens[info.name] = config;
            }
        }
        private void RegisterPropConfig(PropAssetFolderRuleXml config, PropInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                config.Source = RuleSource.ASSET;
                m_colorConfigDataProps[info.name] = config;
            }
        }
        private void RegisterBuildingConfig(BuildingAssetFolderRuleXml config, BuildingInfo info)
        {
            if (config.AssetName?.Split(new char[] { '.' }, 2)?[1] == info.name?.Split(new char[] { '.' }, 2)?[1])
            {
                config.AssetName = info.name;
                config.Source = RuleSource.ASSET;
                m_colorConfigDataBuildings[info.name] = config;
            }
        }
        private void RegisterBuildingPropConfig(PropAssetFolderRuleXml config, BuildingInfo info)
        {
            if (config.AssetName != null)
            {
                config.BuildingName = info.name;
                config.Source = RuleSource.ASSET;
                if (m_colorConfigDataBuildingsProps[info.name] == null)
                {
                    m_colorConfigDataBuildingsProps[info.name] = new Dictionary<string, PropAssetFolderRuleXml>();
                }
                m_colorConfigDataBuildingsProps[info.name][config.AssetName] = config;
            }
        }

        private void RegisterBuildingVehicleConfig(VehicleAssetFolderRuleXml config, BuildingInfo info)
        {
            if (config.AssetName == null)
            {
                config.BuildingName = info.name;
                config.Source = RuleSource.ASSET;
                m_colorConfigDataBuildingsVehiclesGlobal[info.name] = config;
            }
        }

        private void RegisterNetPropConfig(PropAssetFolderRuleXml config, NetInfo info)
        {
            if (config.AssetName != null)
            {
                config.NetName = info.name;
                config.Source = RuleSource.ASSET;
                if (m_colorConfigDataNetsProps[info.name] == null)
                {
                    m_colorConfigDataNetsProps[info.name] = new Dictionary<string, PropAssetFolderRuleXml>();
                }
                m_colorConfigDataNetsProps[info.name][config.AssetName] = config;
            }
        }

        #endregion

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
                                (report.ContainsKey(entry.Key) ? report[entry.Key] : report[entry.Key] = new List<FormattedReportLine>()).Add(new FormattedReportLine("<Self>", entry.Value.Source));
                            }
                            foreach (KeyValuePair<string, PropAssetFolderRuleXml> entry in m_colorConfigDataBuildingsPropsGlobal)
                            {
                                (report.ContainsKey(entry.Key) ? report[entry.Key] : report[entry.Key] = new List<FormattedReportLine>()).Add(new FormattedReportLine("<Global prop>", entry.Value.Source));
                            }
                            foreach (KeyValuePair<string, Dictionary<string, PropAssetFolderRuleXml>> entry in m_colorConfigDataBuildingsProps)
                            {
                                foreach (KeyValuePair<string, PropAssetFolderRuleXml> subEntry in entry.Value)
                                {
                                    (report.ContainsKey(entry.Key) ? report[entry.Key] : report[entry.Key] = new List<FormattedReportLine>()).Add(new FormattedReportLine("(Prop) " + subEntry.Key, subEntry.Value.Source));
                                }
                            }
                            m_cachedLoadedReport[(int)CacheOrder.BUILDING] = report.SelectMany(x => new FormattedReportLine[] { new FormattedReportLine(x.Key, null, 0) }.Union(x.Value)).ToArray();
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
                                (report.ContainsKey(entry.Key) ? report[entry.Key] : report[entry.Key] = new List<FormattedReportLine>()).Add(new FormattedReportLine("<Global prop>", entry.Value.Source));
                            }
                            foreach (KeyValuePair<string, Dictionary<string, PropAssetFolderRuleXml>> entry in m_colorConfigDataNetsProps)
                            {
                                foreach (KeyValuePair<string, PropAssetFolderRuleXml> subEntry in entry.Value)
                                {
                                    (report.ContainsKey(entry.Key) ? report[entry.Key] : report[entry.Key] = new List<FormattedReportLine>()).Add(new FormattedReportLine("(Prop) " + subEntry.Key, subEntry.Value.Source));
                                }
                            }
                            m_cachedLoadedReport[(int)CacheOrder.NET] = report.SelectMany(x => new FormattedReportLine[] { new FormattedReportLine(x.Key, null, 0) }.Union(x.Value)).ToArray();
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
                                (report).Add(new FormattedReportLine(entry.Key, entry.Value.Source, 0));
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
                                (report).Add(new FormattedReportLine(entry.Key, entry.Value.Source, 0));
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
                                (report).Add(new FormattedReportLine(entry.Key, entry.Value.Source, 0));
                            }
                            m_cachedLoadedReport[(int)CacheOrder.CITIZEN] = report.ToArray();
                        }
                    }
                    return ref m_cachedLoadedReport[(int)CacheOrder.CITIZEN];
                default:
                    return ref m_nullArr;
            }
        }
        private FormattedReportLine[] m_nullArr = null;


        internal class FormattedReportLine
        {
            public FormattedReportLine(string text, RuleSource? source, byte level = 1)
            {
                Text = text;
                Level = level;
                Source = source;
            }

            public byte Level { get; private set; }
            public string Text { get; private set; }
            public RuleSource? Source { get; private set; }

            public override string ToString()
            {
                if (Level == 0)
                {
                    return $"\t<color #FFFF00>- {Text}</color> {(Source != null ? $"<color #888888>From: {Source}</color>" : "")}";
                }
                else
                {
                    return $"\t\t<color #FFFFFF>- {Text}</color> {(Source != null ? $"<color #888888>From: {Source}</color>" : "")}";
                }
            }
        }

    }
}