using Klyte.BuildingColorExpander.UI;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

[assembly: AssemblyVersion("1.99.99.*")]
namespace Klyte.BuildingColorExpander
{
    public class BCEController : BaseController<BuildingColorExpanderMod, BCEController>
    {
        public const string DEFAULT_XML_NAME = "k45_bce_data.xml";
        public static readonly string FOLDER_PATH = FileUtils.BASE_FOLDER_PATH + "BuildingColorExpander";
        public const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralXmlConfigs";

        internal readonly Dictionary<string, AssetFolderRulesXml> m_colorConfigData = new Dictionary<string, AssetFolderRulesXml>();
        public readonly Dictionary<string, BasicColorConfigurationXml> m_cachedRules = new Dictionary<string, BasicColorConfigurationXml>();



        protected override void StartActions() => ReloadFiles();

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