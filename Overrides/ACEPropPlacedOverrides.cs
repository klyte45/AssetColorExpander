using ColossalFramework.Math;
using Harmony;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static RenderManager;

namespace Klyte.AssetColorExpander
{
    public class ACEPropPlacedOverrides : Redirector, IRedirectable
    {

        private static MethodInfo GetColorProp { get; } = typeof(PropInfo).GetMethod("GetColor", RedirectorUtils.allFlags, null, new Type[] { typeof(Randomizer).MakeByRefType() }, null);

        public void Awake()
        {
            AddRedirect(typeof(PropInstance).GetMethod("RenderInstance", RedirectorUtils.allFlags, null, new Type[] { typeof(CameraInfo), typeof(ushort), typeof(int) }, null), null, null, typeof(ACEPropPlacedOverrides).GetMethod("DetourRenderInstance", RedirectorUtils.allFlags));
            AddRedirect(typeof(BuildingAI).GetMethod("RenderProps", RedirectorUtils.allFlags, null, new Type[] { typeof(RenderManager.CameraInfo), typeof(ushort), typeof(Building).MakeByRefType(), typeof(int), typeof(RenderManager.Instance).MakeByRefType(), typeof(bool), typeof(bool), typeof(bool) }, null), null, null, typeof(ACEPropPlacedOverrides).GetMethod("DetourBuildingRenderProps", RedirectorUtils.allFlags));
            AddRedirect(typeof(NetLane).GetMethod("RenderInstance", RedirectorUtils.allFlags), null, null, typeof(ACEPropPlacedOverrides).GetMethod("DetourNetLaneRenderInstance", RedirectorUtils.allFlags));

            AddRedirect(typeof(PropManager).GetMethod("ReleaseProp"), null, typeof(ACEPropPlacedOverrides).GetMethod("AfterReleaseProp", RedirectorUtils.allFlags));
        }

        //public static Dictionary<string, BuildingAssetFolderRuleXml> AssetsRules => AssetColorExpanderMod.Controller?.m_colorConfigDataBuildings;
        public static ref Color?[] ColorCachePlaced => ref AssetColorExpanderMod.Controller.CachedColor[(int)ACEController.CacheOrder.PROP_PLACED];
        public static ref bool[] RulesUpdatedPlaced => ref AssetColorExpanderMod.Controller.UpdatedRules[(int)ACEController.CacheOrder.PROP_PLACED];
        public static ref Color?[][] ColorCacheBuilding => ref AssetColorExpanderMod.Controller.CachedColorSubPropsBuildings;
        public static ref bool[][] RulesUpdatedBuilding => ref AssetColorExpanderMod.Controller.UpdatedRulesSubPropsBuildings;
        public static ref Color?[][][] ColorCacheNet => ref AssetColorExpanderMod.Controller.CachedColorSubPropsNets;
        public static ref bool[][][] RulesUpdatedNet => ref AssetColorExpanderMod.Controller.UpdatedRulesSubPropsNets;

        public static Color PreGetColor(ref Randomizer randomizer, uint seed, PropInfo propInfo, ItemClass parentItemClass, BuildingInfo buildingInfo, NetInfo netInfo, Vector3 position, ref Color? colorCacheItem, ref bool rulesUpdatedItem)
        {

            if (rulesUpdatedItem)
            {
                return colorCacheItem ?? propInfo.GetColor(ref randomizer);
            }

            string dataName = propInfo?.name;
            BasicColorConfigurationXml itemData;

            byte district = DistrictManager.instance.GetDistrict(position);
            byte park = DistrictManager.instance.GetPark(position);
            itemData = ACEPropConfigRulesData.Instance.Rules.m_dataArray.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Accepts(propInfo.m_class, parentItemClass, propInfo?.name, buildingInfo?.name, netInfo?.name, district, park)).OrderBy(x => x.First).FirstOrDefault()?.Second;

            rulesUpdatedItem = true;
            Color result = default;

            if (itemData == null || itemData.ColoringMode == ColoringMode.SKIP || ACEColorGenUtils.GetColor(seed, ref result, itemData, ref colorCacheItem, ref rulesUpdatedItem))
            {
                LogUtils.DoLog($"NOT GETTING COLOR FOR PROP: {seed} - \"{itemData?.ColoringMode}\" not available");
                colorCacheItem = null;
                rulesUpdatedItem = true;
                return propInfo.GetColor(ref randomizer);
            }
            else
            {
                LogUtils.DoLog($" GETTING COLOR FOR PROP: {seed} = {result} ");
                return result;
            }

        }

        public static void AfterReleaseProp(ushort prop)
        {
            if (AssetColorExpanderMod.Controller != null && RulesUpdatedPlaced != null)
            {
                RulesUpdatedPlaced[prop] = false;
            }
        }

        public static IEnumerable<CodeInstruction> DetourRenderInstance(IEnumerable<CodeInstruction> instr)
        {
            var instrList = new List<CodeInstruction>(instr);
            for (int i = 0; i < instrList.Count; i++)
            {
                if (instrList[i].operand == GetColorProp)
                {
                    instrList.RemoveAt(i);
                    instrList.InsertRange(i, new List<CodeInstruction>{
                        new CodeInstruction( OpCodes.Ldarg_2),
                        new CodeInstruction( OpCodes.Call, typeof(ACEPropPlacedOverrides).GetMethod("PreGetColorPropId",RedirectorUtils.allFlags)),
                        });
                }
                i += 4;

            }
            LogUtils.PrintMethodIL(instrList);
            return instrList;
        }

        public static IEnumerable<CodeInstruction> DetourBuildingRenderProps(IEnumerable<CodeInstruction> instr, ILGenerator il)
        {
            var instrList = new List<CodeInstruction>(instr);
            for (int i = 0; i < instrList.Count; i++)
            {
                if (instrList[i].operand == GetColorProp)
                {
                    instrList.RemoveAt(i);
                    instrList.InsertRange(i, new List<CodeInstruction>{
                        new CodeInstruction( OpCodes.Ldarg_0),
                        new CodeInstruction( OpCodes.Ldarg_3),
                        new CodeInstruction( OpCodes.Ldarg_2),
                        new CodeInstruction( OpCodes.Ldloc_S, 11),
                        new CodeInstruction( OpCodes.Call, typeof(ACEPropPlacedOverrides).GetMethod("PreGetColorBuildingProps",RedirectorUtils.allFlags)),
                        });
                    i += 4;
                }

            }
            LogUtils.PrintMethodIL(instrList);

            return instrList;
        }
        public static IEnumerable<CodeInstruction> DetourNetLaneRenderInstance(IEnumerable<CodeInstruction> instr)
        {
            var instrList = new List<CodeInstruction>(instr);
            for (int i = 0; i < instrList.Count; i++)
            {
                if (instrList[i].operand == GetColorProp)
                {
                    instrList.RemoveAt(i);
                    instrList.InsertRange(i, new List<CodeInstruction>{
                        new CodeInstruction(OpCodes.Ldarga_S, 0),
                        new CodeInstruction( OpCodes.Ldarg_2),
                        new CodeInstruction( OpCodes.Ldarg_3),
                        new CodeInstruction( OpCodes.Ldarg_S,4),
                        new CodeInstruction( OpCodes.Ldloc_S, 11),
                        new CodeInstruction( OpCodes.Ldloc_S, 20),
                        new CodeInstruction( OpCodes.Call, typeof(ACEPropPlacedOverrides).GetMethod("PreGetColorNetProps",RedirectorUtils.allFlags)),
                        });
                    i += 4;
                }

            }
            LogUtils.PrintMethodIL(instrList);

            return instrList;
        }


        public const uint BUILDINGS_OFFSET_SEED = PropManager.MAX_PROP_COUNT;
        public const uint NETS_OFFSET_SEED = PropManager.MAX_PROP_COUNT + (BuildingManager.MAX_BUILDING_COUNT << 8);


        public static Color PreGetColorPropId(PropInfo info, ref Randomizer randomizer, ushort propId) => PreGetColor(ref randomizer, propId, info, null, null, null, PropManager.instance.m_props.m_buffer[propId].Position, ref ColorCachePlaced[propId], ref RulesUpdatedPlaced[propId]);
        public static Color PreGetColorBuildingProps(PropInfo info, ref Randomizer randomizer, BuildingAI ai, ref Building data, ushort buildingId, int i)
        {
            if (RulesUpdatedBuilding[buildingId] == null)
            {
                RulesUpdatedBuilding[buildingId] = new bool[ai.m_info.m_props.Length];
                ColorCacheBuilding[buildingId] = new Color?[ai.m_info.m_props.Length];
            }
            if (i >= RulesUpdatedBuilding[buildingId].Length)
            {
                if (ai.m_info.m_props.Length != RulesUpdatedBuilding[buildingId]?.Length)
                {
                    RulesUpdatedBuilding[buildingId] = new bool[ai.m_info.m_props.Length];
                }
                if (ai.m_info.m_props.Length != ColorCacheBuilding[buildingId]?.Length)
                {
                    ColorCacheBuilding[buildingId] = new Color?[ai.m_info.m_props.Length];
                }
                if (i >= RulesUpdatedBuilding[buildingId].Length)
                {
                    LogUtils.DoWarnLog($"INVALID PROP IDX({i}) FOR BUILDING {buildingId} ({ai.m_info} - PROPS={ai.m_info.m_props.Length})");
                    return default;
                }
            }
            return PreGetColor(ref randomizer, (uint)(BUILDINGS_OFFSET_SEED + (buildingId << 8) + i), info, ai.m_info.m_class, ai.m_info, null, data.m_position, ref ColorCacheBuilding[buildingId][i], ref RulesUpdatedBuilding[buildingId][i]);
        }


        //   public static Color PreGetColorNetProps(PropInfo info, ref Randomizer randomizer, ref NetLane netLane) => default;//, ushort segmentId, uint laneId, int propId, float position) => default;//


        public static Color PreGetColorNetProps(PropInfo info, ref Randomizer randomizer, ref NetLane netLane, ushort segmentId, uint laneId, NetInfo.Lane laneInfo, int propId, float position)
        {
            if (RulesUpdatedNet[laneId] == null || RulesUpdatedNet[laneId].Length != (laneInfo.m_laneProps?.m_props?.Length ?? 0))
            {
                int propCount = laneInfo.m_laneProps?.m_props?.Length ?? 0;
                RulesUpdatedNet[laneId] = new bool[propCount][];
                ColorCacheNet[laneId] = new Color?[propCount][];
                for (int j = 0; j < propCount; j++)
                {
                    RulesUpdatedNet[laneId][j] = new bool[8];
                    ColorCacheNet[laneId][j] = new Color?[8];
                }
            }
            NetInfo netInfo = NetManager.instance.m_segments.m_buffer[segmentId].Info;
            int idx = Mathf.Min(7, Mathf.FloorToInt(position * 8));
            return PreGetColor(ref randomizer, (uint)(NETS_OFFSET_SEED + (segmentId << 19) + (laneId << 11) + (propId << 3) + Mathf.FloorToInt(position * 8)), info, netInfo.m_class, null, netInfo, netLane.m_bezier.Position(position), ref ColorCacheNet[laneId][propId][idx], ref RulesUpdatedNet[laneId][propId][idx]);
        }
    }
}