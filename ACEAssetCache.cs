using ColossalFramework;
using ColossalFramework.Globalization;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Klyte.AssetColorExpander
{
    public class ACEAssetCache
    {

        #region Prefab loading

        private Dictionary<string, string> m_vehiclesLoaded, m_buildingsLoaded, m_citizensLoaded, m_propsLoaded, m_netsLoaded;

        public Dictionary<string, string> VehiclesLoaded
        {
            get {
                if (m_vehiclesLoaded == null)
                {
                    m_vehiclesLoaded = GetInfos<VehicleInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_vehiclesLoaded;
            }
        }

        public Dictionary<string, string> BuildingsLoaded
        {
            get {
                if (m_buildingsLoaded == null)
                {
                    m_buildingsLoaded = GetInfos<BuildingInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_buildingsLoaded;
            }
        }

        public Dictionary<string, string> CitizensLoaded
        {
            get {
                if (m_citizensLoaded == null)
                {
                    m_citizensLoaded = GetInfos<CitizenInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_citizensLoaded;


            }
        }
        public Dictionary<string, string> PropsLoaded
        {
            get {
                if (m_propsLoaded == null)
                {
                    m_propsLoaded = GetInfos<PropInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_propsLoaded;
            }
        }
        public Dictionary<string, string> NetsLoaded
        {
            get {
                if (m_netsLoaded == null)
                {
                    m_netsLoaded = GetInfos<NetInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_netsLoaded;
            }
        }


        private static string GetListName(PrefabInfo x) => (x?.name?.EndsWith("_Data") ?? false) ? $"{x?.GetLocalizedTitle()} (id: {x.name.Split('.')[0]})" : x?.name ?? "";
        private List<T> GetInfos<T>() where T : PrefabInfo
        {
            var list = new List<T>();
            uint num = 0u;
            while (num < (ulong)PrefabCollection<T>.LoadedCount())
            {
                T prefabInfo = PrefabCollection<T>.GetLoaded(num);
                if (prefabInfo != null)
                {
                    list.Add(prefabInfo);
                }
                num += 1u;
            }
            return list;
        }

        public string[] FilterVehiclesByText(string text) => VehiclesLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();

        public string[] FilterBuildingsByText(string text) => BuildingsLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        public string[] FilterCitizensByText(string text) => CitizensLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        public string[] FilterPropsByText(string text) => PropsLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        public string[] FilterNetsByText(string text) => NetsLoaded
            .ToList()
            .Where((x) => text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();
        #endregion
    }
}