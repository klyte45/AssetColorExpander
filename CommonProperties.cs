using Klyte.BuildingColorExpander;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => BuildingColorExpanderMod.DebugMode;
        public static string Version => BuildingColorExpanderMod.Version;
        public static string ModName => BuildingColorExpanderMod.Instance.SimpleName;
        public static string Acronym => "BCE";
        public static string ModRootFolder => BCEController.FOLDER_PATH;
        public static string ModIcon => BuildingColorExpanderMod.Instance.IconName; 
        public static string ModDllRootFolder => BuildingColorExpanderMod.RootFolder;
    }
}