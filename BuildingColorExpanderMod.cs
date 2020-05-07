using ColossalFramework.Math;
using ColossalFramework.UI;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

[assembly: AssemblyVersion("1.99.99.*")]
namespace Klyte.BuildingColorExpander
{
    public class BCEController : BaseController<BuildingColorExpanderMod, BCEController>
    {
        public static string DefaultBuildingsConfigurationFolder { get; } = FOLDER_PATH + Path.DirectorySeparatorChar + DEFAULT_CUSTOM_CONFIG_FOLDER;
        public const string DEFAULT_XML_NAME = "k45_bce_data.xml";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + "BuildingColorExpander";
        public const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralXmlConfigs";

        internal readonly Dictionary<string, ColorConfigurationXml> m_colorConfigData = new Dictionary<string, ColorConfigurationXml>();
        protected override void StartActions() => ReloadFiles();

        private void ReloadFiles()
        {
            FileUtils.EnsureFolderCreation(DefaultBuildingsConfigurationFolder);
            m_colorConfigData.Clear();
            LoadAllBuildingConfigurations();
            if (BuildingColorExpanderMod.DebugMode)
            {
                var serializer = new XmlSerializer(typeof(ColorConfigurationXml));
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



        public void LoadAllBuildingConfigurations()
        {
            FileUtils.ScanPrefabsFolders<BuildingInfo>($"{DEFAULT_XML_NAME}.xml", LoadDescriptorsFromXml);
            foreach (string filename in Directory.GetFiles(DefaultBuildingsConfigurationFolder, "*.xml"))
            {
                using FileStream stream = File.OpenRead(filename);
                LoadDescriptorsFromXml(stream, null);
            }

        }

        private void LoadDescriptorsFromXml(FileStream stream, BuildingInfo info)
        {
            var serializer = new XmlSerializer(typeof(BceConfig));

            if (serializer.Deserialize(stream) is BceConfig configList)
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

    }
    public class BCEPanel : BasicKPanel<BuildingColorExpanderMod, BCEController, BCEPanel>
    {
        public override float PanelWidth => GetComponentInParent<UIComponent>().width;

        public override float PanelHeight => GetComponentInParent<UIComponent>().height;

        protected override void AwakeActions() { }
    }

    public class BCEOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(BuildingAI).GetMethod("GetColor"), typeof(BCEOverrides).GetMethod("PreGetColor", RedirectorUtils.allFlags));

        public static Dictionary<string, ColorConfigurationXml> ColorConfigData => BuildingColorExpanderMod.Controller?.m_colorConfigData;

        public static bool PreGetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode, ref Color __result)
        {
            if (infoMode != InfoManager.InfoMode.None || ColorConfigData == null)
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR BUILDING: {buildingID} INFO = {infoMode}");
                return true;
            }
            if (!ColorConfigData.TryGetValue(data.Info.name, out ColorConfigurationXml itemData)
                && !ColorConfigData.TryGetValue(data.Info.m_buildingAI.GetType().Name, out itemData)
                && (!(data.Info.m_buildingAI is PrivateBuildingAI) || !ColorConfigData.TryGetValue("__ZONED__", out itemData))
                && !ColorConfigData.TryGetValue("*", out itemData))
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

                    __result = itemData.ColorList[randomizer.Int32((uint)itemData.ColorList.Count)];
                    LogUtils.DoLog($"GETTING LIST COLOR: {__result}");
                    return false;
                default:

                    LogUtils.DoLog($"GETTING DEFAULT COLOR!");
                    return true;
            }
        }
    }
    public class BuildingColorExpanderMod : BasicIUserMod<BuildingColorExpanderMod, BCEController, BCEPanel>
    {
        public override string SimpleName => "Building Color Expander";

        public override string Description => "Expand the color variation of the buildings";

        public override void TopSettingsUI(UIHelperExtension helper)
        {
            //UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("K45_BCE_GENERAL_INFO"));
            //AddFolderButton(DefaultBuildingsConfigurationFolder, group8, "K45_BCE_DEFAULT_BUILDINGS_CONFIG_PATH_TITLE");
            //helper.AddButton(Locale.Get("K45_BCE_RELOAD_FILES"), ReloadFiles);
        }

        //private static void AddFolderButton(string filePath, UIHelperExtension helper, string localeId)
        //{
        //    FileInfo fileInfo = FileUtils.EnsureFolderCreation(filePath);
        //    helper.AddLabel(Locale.Get(localeId) + ":");
        //    var namesFilesButton = ((UIButton)helper.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName)));
        //    namesFilesButton.textColor = Color.yellow;
        //    KlyteMonoUtils.LimitWidth(namesFilesButton, 710);
        //    namesFilesButton.text = fileInfo.FullName + Path.DirectorySeparatorChar;
        //}


    }
}