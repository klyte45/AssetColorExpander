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
    }
}