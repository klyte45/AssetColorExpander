using Klyte.BuildingColorExpander;

namespace Klyte.Commons
{
    public static class CommonProperties
    {
        public static bool DebugMode => BuildingColorExpanderMod.DebugMode;
        public static string Version => BuildingColorExpanderMod.Version;
        public static string ModName => BuildingColorExpanderMod.Instance.SimpleName;
        public static object Acronym => "BCE";
    }
}