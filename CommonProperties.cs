using Klyte.AssetColorExpander;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => AssetColorExpanderMod.DebugMode;
        public static string Version => AssetColorExpanderMod.Version;
        public static string ModName => AssetColorExpanderMod.Instance.SimpleName;
        public static string Acronym => "ACE";
        public static string ModRootFolder => ACEController.FOLDER_PATH;
        public static string ModIcon => AssetColorExpanderMod.Instance.IconName;
        public static string ModDllRootFolder => AssetColorExpanderMod.RootFolder;

        public static string GitHubRepoPath => "klyte45/AssetColorExpander";


        internal static readonly string[] AssetExtraDirectoryNames = new string[0];
        internal static readonly string[] AssetExtraFileNames = new string[] {
            ACELoadedDataContainer.DEFAULT_XML_NAME_BUILDING ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_BUILDING_PROPS ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_BUILDING_PROPS_GLOBAL ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_BUILDING_VEHICLES ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_BUILDING_VEHICLES_GLOBAL ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_NET_PROPS ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_NET_PROPS_GLOBAL ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_VEHICLE ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_CITIZEN ,
            ACELoadedDataContainer.DEFAULT_XML_NAME_PROP ,
        };

    }
}