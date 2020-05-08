using Klyte.BuildingColorExpander.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: AssemblyVersion("1.99.99.*")]
namespace Klyte.BuildingColorExpander
{
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