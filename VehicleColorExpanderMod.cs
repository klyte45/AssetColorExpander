using ColossalFramework.Globalization;
using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using Klyte.VehicleColorExpander.Utils;
using Klyte.VehicleColorExpander.XML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.VehicleColorExpander
{
    public class VehicleColorExpanderMod : BasicIUserModSimplified<VehicleColorExpanderMod, VCEResourceLoader, MonoBehaviour>
    {
        public VehicleColorExpanderMod() => Construct();

        public override string SimpleName => "Vehicle Color Expander";

        public override string Description => "Expand the color variation of the vehicles";

        public override void DoErrorLog(string fmt, params object[] args) => LogUtils.DoErrorLog(fmt, args);

        public override void DoLog(string fmt, params object[] args) => LogUtils.DoLog(fmt, args);

        public override void LoadSettings() => m_redirector = KlyteMonoUtils.CreateElement<Redirector>(null, "VCE");

        public override void TopSettingsUI(UIHelperExtension helper)
        {
            UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("K45_VCE_GENERAL_INFO"));
            AddFolderButton(DefaultBuildingsConfigurationFolder, group8, "K45_VCE_DEFAULT_BUILDINGS_CONFIG_PATH_TITLE");
            helper.AddButton(Locale.Get("K45_VCE_RELOAD_FILES"), ReloadFiles);
        }

        private static void AddFolderButton(string filePath, UIHelperExtension helper, string localeId)
        {
            FileInfo fileInfo = FileUtils.EnsureFolderCreation(filePath);
            helper.AddLabel(Locale.Get(localeId) + ":");
            UIButton namesFilesButton = ((UIButton) helper.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName)));
            namesFilesButton.textColor = Color.yellow;
            KlyteMonoUtils.LimitWidth(namesFilesButton, 710);
            namesFilesButton.text = fileInfo.FullName + Path.DirectorySeparatorChar;
        }

        public const string DEFAULT_XML_NAME = "k45_vce_data.xml";
        public static readonly string FOLDER_NAME = FileUtils.BASE_FOLDER_PATH + "VehicleColorExpander";
        public const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralXmlConfigs";

        public static string DefaultBuildingsConfigurationFolder { get; } = FOLDER_NAME + Path.DirectorySeparatorChar + DEFAULT_CUSTOM_CONFIG_FOLDER;

        private static readonly Dictionary<string, ColorConfigurationXml> m_colorConfigData = new Dictionary<string, ColorConfigurationXml>();
        protected override void OnLevelLoadingInternal()
        {
            m_redirector.AddRedirect(typeof(VehicleAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), typeof(VehicleColorExpanderMod).GetMethod("PreGetColor", RedirectorUtils.allFlags));
            m_redirector.AddRedirect(typeof(VehicleAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(VehicleParked).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), typeof(VehicleColorExpanderMod).GetMethod("PreGetColorParked", RedirectorUtils.allFlags));
            m_redirector.AddRedirect(typeof(PassengerCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(Vehicle).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), typeof(VehicleColorExpanderMod).GetMethod("PreGetColor", RedirectorUtils.allFlags));
            m_redirector.AddRedirect(typeof(PassengerCarAI).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(ushort), typeof(VehicleParked).MakeByRefType(), typeof(InfoManager.InfoMode) }, null), typeof(VehicleColorExpanderMod).GetMethod("PreGetColorParked", RedirectorUtils.allFlags));
            ReloadFiles();
        }

        private void ReloadFiles()
        {
            m_colorConfigData.Clear();
            LoadAllBuildingConfigurations();
            if (DebugMode)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ColorConfigurationXml));
                DoLog($"itemCount = {m_colorConfigData.Count} \r\n" + string.Join("\r\n", m_colorConfigData.Select((x) =>
                  {
                      StringWriter strWriter = new StringWriter();
                      serializer.Serialize(strWriter, x.Value);
                      string val = strWriter.ToString();
                      strWriter.Close();
                      return $"{x.Key} => [ \r\n{val}\r\n ]";
                  }).ToArray()));
            }
        }

        public static bool PreGetColor(ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {vehicleID} INFO = {infoMode}");
                return true;
            }
            if (!m_colorConfigData.TryGetValue(data.Info.name, out ColorConfigurationXml itemData)
                && !m_colorConfigData.TryGetValue(data.Info.m_vehicleAI.GetType().Name, out itemData)
                && !m_colorConfigData.TryGetValue("*", out itemData))
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {vehicleID} ({data.Info.m_vehicleAI.GetType().Name} | {data.Info.name}) - not found");
                return true;
            }
            LogUtils.DoLog($"GETTING COLOR FOR VEHICLE: {vehicleID}  ({data.Info.m_vehicleAI.GetType().Name} | {data.Info.name}) ");
            return GetNewColor(vehicleID, ref __result, itemData);
        }
        public static bool PreGetColorParked(ushort parkedVehicleID, ref VehicleParked data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (infoMode != InfoManager.InfoMode.None)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {parkedVehicleID} INFO = {infoMode}");
                return true;
            }
            if (!m_colorConfigData.TryGetValue(data.Info.name, out ColorConfigurationXml itemData)
                && !m_colorConfigData.TryGetValue(data.Info.m_vehicleAI.GetType().Name, out itemData)
                && !m_colorConfigData.TryGetValue("*", out itemData))
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR VEHICLE: {parkedVehicleID} ({data.Info.m_vehicleAI.GetType().Name} | {data.Info.name}) - not found");
                return true;
            }
            LogUtils.DoLog($"GETTING COLOR FOR VEHICLE: {parkedVehicleID}  ({data.Info.m_vehicleAI.GetType().Name} | {data.Info.name}) ");
            return GetNewColor(parkedVehicleID, ref __result, itemData);
        }

        private static bool GetNewColor(ushort vehicleID, ref Color __result, ColorConfigurationXml itemData)
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
                    __result = new RandomPastelColorGenerator(vehicleID, multiplier, itemData.PastelConfig).GetNext();
                    LogUtils.DoLog($"GETTING PASTEL COLOR: {__result}");
                    return false;

                case ColoringMode.LIST:
                    if (itemData.ColorList.Count == 0)
                    {
                        LogUtils.DoLog($"NO COLOR AVAILABLE!");
                        return true;
                    }
                    Randomizer randomizer = new Randomizer(vehicleID);

                    __result = itemData.ColorList[randomizer.Int32((uint) itemData.ColorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {__result}");
                    return false;
                default:

                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    return true;
            }
        }

        public void LoadAllBuildingConfigurations()
        {
            FileUtils.ScanPrefabsFolders($"{DEFAULT_XML_NAME}.xml", LoadDescriptorsFromXml);
            foreach (string filename in Directory.GetFiles(DefaultBuildingsConfigurationFolder, "*.xml"))
            {
                using FileStream stream = File.OpenRead(filename);
                LoadDescriptorsFromXml(stream);
            }

        }

        private void LoadDescriptorsFromXml(FileStream stream)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(VceConfig));

            if (serializer.Deserialize(stream) is VceConfig configList)
            {
                foreach (ColorConfigurationXml config in configList.ConfigList)
                {
                    if (!string.IsNullOrEmpty(config.AssetName))
                    {
                        m_colorConfigData[config.AssetName] = config;
                    }
                }
            }
        }


        private Redirector m_redirector;
    }
}