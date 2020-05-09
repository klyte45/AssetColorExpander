using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class VehicleCityDataRuleXml : BasicVehicleColorConfigurationXml
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
        public string AssetNameVehicle { get; set; }
        [XmlAttribute]
        public string AssetNameBuilding { get; set; }

        [XmlAttribute]
        public RuleCheckTypeVehicle RuleCheckType { get; set; }

        internal bool Accepts(ushort vehicleId, VehicleInfo info)
        {
            switch (RuleCheckType)
            {
                case RuleCheckTypeVehicle.ITEM_CLASS:
                    return info.m_class.name == ItemClassName;
                case RuleCheckTypeVehicle.SERVICE:
                    return Service == ItemClass.Service.None || info.m_class.m_service == Service;
                case RuleCheckTypeVehicle.SERVICE_SUBSERVICE:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService;
                case RuleCheckTypeVehicle.SERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_level == Level;
                case RuleCheckTypeVehicle.SERVICE_SUBSERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService && info.m_class.m_level == Level;
                case RuleCheckTypeVehicle.ASSET_NAME_SELF:
                    return info.name == AssetNameVehicle;
                case RuleCheckTypeVehicle.ASSET_NAME_OWNER:
                    ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                    return vehicle.m_sourceBuilding > 0 && BuildingManager.instance.m_buildings.m_buffer[vehicle.m_sourceBuilding].Info.name == AssetNameBuilding;
            }

            return false;
        }
        internal bool AcceptsParked(ushort vehicleId, VehicleInfo info)
        {
            switch (RuleCheckType)
            {
                case RuleCheckTypeVehicle.ITEM_CLASS:
                    return info.m_class.name == ItemClassName;
                case RuleCheckTypeVehicle.SERVICE:
                    return Service == ItemClass.Service.None || info.m_class.m_service == Service;
                case RuleCheckTypeVehicle.SERVICE_SUBSERVICE:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService;
                case RuleCheckTypeVehicle.SERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_level == Level;
                case RuleCheckTypeVehicle.SERVICE_SUBSERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService && info.m_class.m_level == Level;
                case RuleCheckTypeVehicle.ASSET_NAME_SELF:
                    return info.name == AssetNameVehicle;
                case RuleCheckTypeVehicle.ASSET_NAME_OWNER:
                    return false;
            }

            return false;
        }
    }

}