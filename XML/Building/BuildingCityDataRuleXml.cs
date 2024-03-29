﻿using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    [XmlRoot("BuildingColorConfig")]
    public class BuildingCityDataRuleXml : BasicColorConfigurationXml
    {
        [XmlAttribute]
        public string ItemClassName { get; set; }
        [XmlAttribute]
        public ItemClass.Service Service { get; set; }
        [XmlAttribute]
        public ItemClass.SubService SubService { get; set; }
        [XmlAttribute]
        public ItemClass.Level Level { get; set; }
        [XmlAttribute]
        public string AssetName { get; set; }

        [XmlArray("SelectedDistricts")] [XmlArrayItem("District")] public HashSet<ushort> SelectedDistricts { get; set; } = new HashSet<ushort>();
        [XmlAttribute("districtSelectionIsBlacklist")] public bool SelectedDistrictsIsBlacklist { get; set; } = true;
        [XmlAttribute("districtRestrictionOrder")] public DistrictRestrictionOrder DistrictRestrictionOrder { get; set; }

        [XmlAttribute]
        public RuleCheckTypeBuilding RuleCheckType { get; set; }

        internal bool Accepts(BuildingInfo info, byte district, byte park)
        {
            if (Allows(district, park))
            {
                switch (RuleCheckType)
                {
                    case RuleCheckTypeBuilding.ITEM_CLASS:
                        return info.m_class.name == ItemClassName;
                    case RuleCheckTypeBuilding.SERVICE:
                        return Service == ItemClass.Service.None || info.m_class.m_service == Service;
                    case RuleCheckTypeBuilding.SERVICE_SUBSERVICE:
                        return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService;
                    case RuleCheckTypeBuilding.SERVICE_LEVEL:
                        return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_level == Level;
                    case RuleCheckTypeBuilding.SERVICE_SUBSERVICE_LEVEL:
                        return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService && info.m_class.m_level == Level;
                    case RuleCheckTypeBuilding.ASSET_NAME:
                        return info.name == AssetName;
                }
            }
            return false;
        }
        private bool Allows(byte district, byte park)
        {
            if (district == 0 && park > 0)
            {
                return AllowsPark(park);
            }
            if (park == 0)
            {
                return AllowsDistrict(district);
            }
            switch (DistrictRestrictionOrder)
            {
                case DistrictRestrictionOrder.ParksOrDistricts:
                default:
                    return AllowsDistrict(district) || AllowsPark(park);
                case DistrictRestrictionOrder.ParksAndDistricts:
                    return AllowsDistrict(district) && AllowsPark(park);
            }
        }
        public bool AllowsDistrict(byte districtId) => SelectedDistricts.Contains(districtId) != SelectedDistrictsIsBlacklist;
        public bool AllowsPark(byte districtId) => SelectedDistricts.Contains((ushort)(0x100 | districtId)) != SelectedDistrictsIsBlacklist;
    }

}