using ColossalFramework.UI;
using Klyte.Commons.Utils;
using System;

namespace Klyte.AssetColorExpander
{
    public class ACEAssetCache
    {
        public static string GetFromInfoIndex<Idx, Info>(string input, int arg1, string[] arg2, UITextField targetTextField, Action<string, Info> doWithResult) where Idx : PrefabIndexesAbstract<Info, Idx> where Info : PrefabInfo
        {
            string result = arg1 < 0 ? input : arg2[arg1];
            var vehicleInfo = PrefabIndexesAbstract<Info, Idx>.instance.PrefabsLoaded.TryGetValue(result, out Info info) ? info : null;
            doWithResult(result, vehicleInfo);
            targetTextField.text = result ?? "";
            return result;
        }

        #region Prefab loading
        public string[] FilterVehiclesByText(string text) => VehiclesIndexes.instance.BasicInputFiltering(text);
        public string[] FilterBuildingsByText(string text) => BuildingIndexes.instance.BasicInputFiltering(text);
        public string[] FilterCitizensByText(string text) => CitizenIndexes.instance.BasicInputFiltering(text);
        public string[] FilterPropsByText(string text) => PropIndexes.instance.BasicInputFiltering(text);
        public string[] FilterNetsByText(string text) => NetIndexes.instance.BasicInputFiltering(text);
        #endregion
    }
}