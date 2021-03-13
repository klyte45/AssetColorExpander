using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class CitizenCityDataRuleXml : BasicColorConfigurationXml
    {
        [XmlAttribute]
        public string ItemClassName { get; set; }
        [XmlAttribute]
        public string AiClassName { get; set; }
        [XmlAttribute]
        public ItemClass.Service Service { get; set; }
        [XmlAttribute]
        public ItemClass.SubService SubService { get; set; }
        [XmlAttribute]
        public ItemClass.Level Level { get; set; }
        [XmlAttribute]
        public string AssetName { get; set; }

        [XmlAttribute]
        public RuleCheckTypeCitizen RuleCheckType { get; set; }

        internal bool Accepts(CitizenInfo info)
        {
            switch (RuleCheckType)
            {
                case RuleCheckTypeCitizen.ITEM_CLASS:
                    return info.m_class.name == ItemClassName;
                case RuleCheckTypeCitizen.SERVICE:
                    return Service == ItemClass.Service.None || info.m_class.m_service == Service;
                case RuleCheckTypeCitizen.SERVICE_SUBSERVICE:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService;
                case RuleCheckTypeCitizen.SERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_level == Level;
                case RuleCheckTypeCitizen.SERVICE_SUBSERVICE_LEVEL:
                    return (Service == ItemClass.Service.None || info.m_class.m_service == Service) && info.m_class.m_subService == SubService && info.m_class.m_level == Level;
                case RuleCheckTypeCitizen.ASSET_NAME:
                    return info.name == AssetName;
                case RuleCheckTypeCitizen.AI:
                    return AiClassName == "HumanAI" ? info.m_citizenAI is HumanAI : AiClassName == "AnimalAI" ? info.m_citizenAI is AnimalAI : info.m_citizenAI.GetType().Name == AiClassName;
            }
            return false;
        }
    }

}