using ColossalFramework.IO;
using ColossalFramework.Math;
using Klyte.AssetColorExpander.UI;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

[assembly: AssemblyVersion("0.0.0.*")]
namespace Klyte.AssetColorExpander
{
    public class AssetColorExpanderMod : BasicIUserMod<AssetColorExpanderMod, ACEController, ACEPanel>
    {

        public override string SimpleName => "Asset Color Expander";

        public override string Description => "Expand the color variation of assets by type";

        public override void TopSettingsUI(UIHelperExtension helper)
        {
            //UIHelperExtension group8 = helper.AddGroupExtended(Locale.Get("K45_ACE_GENERAL_INFO"));
            //AddFolderButton(DefaultBuildingsConfigurationFolder, group8, "K45_ACE_DEFAULT_BUILDINGS_CONFIG_PATH_TITLE");
            //helper.AddButton(Locale.Get("K45_ACE_RELOAD_FILES"), ReloadFiles);
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