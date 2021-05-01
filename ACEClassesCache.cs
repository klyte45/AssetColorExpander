using Klyte.Commons.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Klyte.AssetColorExpander
{
    public class ACEClassesCache
    {

        public Dictionary<ItemClass, List<BuildingInfo>> AllClassesBuilding { get; private set; }
        public Dictionary<ItemClass, List<VehicleInfo>> AllClassesVehicle { get; private set; }
        public Dictionary<ItemClass, List<CitizenInfo>> AllClassesCitizen { get; private set; }
        public Dictionary<ItemClass, List<PropInfo>> AllClassesProp { get; private set; }
        public Dictionary<ItemClass, List<NetInfo>> AllClassesNet { get; private set; }
        public Dictionary<Type, List<CitizenInfo>> AllAICitizen { get; private set; }

        internal void LoadCache()
        {
            AllClassesBuilding = ((FastList<PrefabCollection<BuildingInfo>.PrefabData>)typeof(PrefabCollection<BuildingInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());

            AllClassesVehicle = ((FastList<PrefabCollection<VehicleInfo>.PrefabData>)typeof(PrefabCollection<VehicleInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());

            AllClassesCitizen = ((FastList<PrefabCollection<CitizenInfo>.PrefabData>)typeof(PrefabCollection<CitizenInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());
            AllClassesProp = ((FastList<PrefabCollection<PropInfo>.PrefabData>)typeof(PrefabCollection<PropInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());
            AllClassesNet = ((FastList<PrefabCollection<NetInfo>.PrefabData>)typeof(PrefabCollection<NetInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_class.name)
              .ToDictionary(x => x.First().m_class, x => x.ToList());

            AllAICitizen = ((FastList<PrefabCollection<CitizenInfo>.PrefabData>)typeof(PrefabCollection<CitizenInfo>).GetField("m_scenePrefabs", RedirectorUtils.allFlags).GetValue(null))
              .m_buffer
              .Select(x => x.m_prefab)
              .Where(x => x?.m_class != null)
              .GroupBy(x => x.m_citizenAI.GetType())
              .ToDictionary(x => x.First().m_citizenAI.GetType(), x => x.ToList());
            AllAICitizen[typeof(HumanAI)] = null;
            AllAICitizen[typeof(AnimalAI)] = null;
        }
    }
}