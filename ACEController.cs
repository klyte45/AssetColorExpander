using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.IO;
using UnityEngine;

namespace Klyte.AssetColorExpander
{

    public class ACEController : BaseController<AssetColorExpanderMod, ACEController>
    {
        public static readonly string FOLDER_PATH = Path.Combine(FileUtils.BASE_FOLDER_PATH, "AssetColorExpander");
        public static readonly string FOLDER_PATH_GENERAL_CONFIG = Path.Combine(FOLDER_PATH, DEFAULT_CUSTOM_CONFIG_FOLDER);
        protected const string DEFAULT_CUSTOM_CONFIG_FOLDER = "GeneralConfigs";

        public ACELoadedDataContainer LoadedConfiguration { get; } = new ACELoadedDataContainer();
        public ACEClassesCache ClassesCache { get; } = new ACEClassesCache();
        public ACEAssetCache AssetsCache { get; } = new ACEAssetCache();

        public Color?[][] CachedColor { get; private set; } = new Color?[Enum.GetValues(typeof(CacheOrder)).Length][];

        public bool[][] UpdatedRules { get; private set; } = new bool[Enum.GetValues(typeof(CacheOrder)).Length][];

        public enum CacheOrder
        {
            BUILDING,
            VEHICLE,
            PARKED_VEHICLE,
            CITIZEN,
            PROP_PLACED,
            NET
        }

        public enum CacheOrderSubprops
        {
            BUILDING,
            NETS
        }

        public Color?[][] CachedColorSubPropsBuildings = null;
        public bool[][] UpdatedRulesSubPropsBuildings = null;

        public Color?[][][] CachedColorSubPropsNets = null;
        public bool[][][] UpdatedRulesSubPropsNets = null;

        protected void Awake() => ClassesCache.LoadCache();

        protected override void StartActions()
        {
            LogUtils.DoWarnLog($"StartActions!");
            LoadedConfiguration.ReloadFiles();
            CleanCache();
        }

        #region cache
        public void CleanCache()
        {
            CleanCacheVehicle();
            CleanCacheBuilding();
            CleanCacheCitizen();
            CleanCacheProp();
        }
        public void CleanCacheVehicle()
        {
            UpdatedRules[(int)CacheOrder.VEHICLE] = new bool[VehicleManager.MAX_VEHICLE_COUNT];
            UpdatedRules[(int)CacheOrder.PARKED_VEHICLE] = new bool[VehicleManager.MAX_PARKED_COUNT];
            CachedColor[(int)CacheOrder.VEHICLE] = new Color?[VehicleManager.MAX_VEHICLE_COUNT];
            CachedColor[(int)CacheOrder.PARKED_VEHICLE] = new Color?[VehicleManager.MAX_PARKED_COUNT];
        }

        public void CleanCacheBuilding()
        {
            UpdatedRules[(int)CacheOrder.BUILDING] = new bool[BuildingManager.MAX_BUILDING_COUNT];
            CachedColor[(int)CacheOrder.BUILDING] = new Color?[BuildingManager.MAX_BUILDING_COUNT];
        }

        public void CleanCacheCitizen()
        {
            UpdatedRules[(int)CacheOrder.CITIZEN] = new bool[CitizenManager.MAX_INSTANCE_COUNT];
            CachedColor[(int)CacheOrder.CITIZEN] = new Color?[CitizenManager.MAX_INSTANCE_COUNT];
        }
        public void CleanCacheProp()
        {
            UpdatedRules[(int)CacheOrder.PROP_PLACED] = new bool[PropManager.MAX_PROP_COUNT];
            CachedColor[(int)CacheOrder.PROP_PLACED] = new Color?[PropManager.MAX_PROP_COUNT];
            UpdatedRulesSubPropsBuildings = new bool[BuildingManager.MAX_BUILDING_COUNT][];
            CachedColorSubPropsBuildings = new Color?[BuildingManager.MAX_BUILDING_COUNT][];
            UpdatedRulesSubPropsNets = new bool[NetManager.MAX_LANE_COUNT][][];
            CachedColorSubPropsNets = new Color?[NetManager.MAX_LANE_COUNT][][];
        }
        #endregion

    }
}