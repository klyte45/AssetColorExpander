using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class PropCityDataRuleXml : BasicColorConfigurationXml
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
        public string AssetNameSelf { get; set; }
        [XmlAttribute]
        public string AssetNameBuilding { get; set; }
        [XmlAttribute]
        public string AssetNameNet { get; set; }

        [XmlArray("SelectedDistricts")] [XmlArrayItem("District")] public HashSet<ushort> SelectedDistricts { get; set; } = new HashSet<ushort>();
        [XmlAttribute("districtSelectionIsBlacklist")] public bool SelectedDistrictsIsBlacklist { get; set; } = true;
        [XmlAttribute("districtRestrictionOrder")] public DistrictRestrictionOrder DistrictRestrictionOrder { get; set; }

        [XmlAttribute]
        public RuleCheckTypeProp RuleCheckType { get; set; }

        internal bool Accepts(ItemClass itemClass, ItemClass parentItemClass, string propInfoName, string buildingName, string netName)
        {
            ItemClass effectiveItemClass = itemClass;
            switch (RuleCheckType)
            {
                case RuleCheckTypeProp.PARENT_SERVICE:
                case RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE:
                case RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE_LEVEL:
                case RuleCheckTypeProp.PARENT_SERVICE_LEVEL:
                case RuleCheckTypeProp.PARENT_ITEM_CLASS:
                    if (parentItemClass == null)
                    {
                        return false;
                    }
                    effectiveItemClass = parentItemClass;
                    break;
            }

            switch (RuleCheckType)
            {
                case RuleCheckTypeProp.ITEM_CLASS:
                case RuleCheckTypeProp.PARENT_ITEM_CLASS:
                    return effectiveItemClass.name == ItemClassName;
                case RuleCheckTypeProp.SERVICE:
                case RuleCheckTypeProp.PARENT_SERVICE:
                    return Service == ItemClass.Service.None || effectiveItemClass.m_service == Service;
                case RuleCheckTypeProp.SERVICE_SUBSERVICE:
                case RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE:
                    return (Service == ItemClass.Service.None || effectiveItemClass.m_service == Service) && effectiveItemClass.m_subService == SubService;
                case RuleCheckTypeProp.SERVICE_LEVEL:
                case RuleCheckTypeProp.PARENT_SERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || effectiveItemClass.m_service == Service) && effectiveItemClass.m_level == Level;
                case RuleCheckTypeProp.SERVICE_SUBSERVICE_LEVEL:
                case RuleCheckTypeProp.PARENT_SERVICE_SUBSERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || effectiveItemClass.m_service == Service) && effectiveItemClass.m_subService == SubService && effectiveItemClass.m_level == Level;
                case RuleCheckTypeProp.ASSET_NAME_SELF:
                    return propInfoName != null && propInfoName == AssetNameSelf;
                case RuleCheckTypeProp.ASSET_NAME_BUILDING:
                    return buildingName != null && buildingName == AssetNameBuilding;
                case RuleCheckTypeProp.ASSET_NAME_NET:
                    return netName != null && netName == AssetNameNet;
            }
            return false;
        }
    }

}