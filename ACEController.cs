using Klyte.AssetColorExpander.ModShared;
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
        internal IBridgeADR ConnectorADR { get; private set; }


        public Color?[][] CachedColor { get; private set; } = new Color?[Enum.GetValues(typeof(CacheOrder)).Length][];

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

        public Color?[][][] CachedColorSubPropsNets = null;

        protected void Awake()
        {
            ClassesCache.LoadCache();
            ConnectorADR = PluginUtils.GetImplementationTypeForMod<BridgeADR, BridgeADRFallback, IBridgeADR>(gameObject, "KlyteAddresses", "3.0.0.3");
        }

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
            CachedColor[(int)CacheOrder.VEHICLE] = new Color?[VehicleManager.instance.m_vehicles.m_size];
            CachedColor[(int)CacheOrder.PARKED_VEHICLE] = new Color?[VehicleManager.instance.m_parkedVehicles.m_size];
        }

        public void CleanCacheBuilding()
        {
            CachedColor[(int)CacheOrder.BUILDING] = new Color?[BuildingManager.MAX_BUILDING_COUNT];
            BuildingManager.instance.UpdateBuildingColors();
        }

        public void CleanCacheCitizen() => CachedColor[(int)CacheOrder.CITIZEN] = new Color?[CitizenManager.MAX_INSTANCE_COUNT];
        public void CleanCacheProp()
        {
            CachedColor[(int)CacheOrder.PROP_PLACED] = new Color?[PropManager.MAX_PROP_COUNT];
            CachedColorSubPropsBuildings = new Color?[BuildingManager.MAX_BUILDING_COUNT][];
            CachedColorSubPropsNets = new Color?[NetManager.MAX_LANE_COUNT][][];
        }
        #endregion

    }
}