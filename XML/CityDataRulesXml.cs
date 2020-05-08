using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.XML
{
    public class CityDataRulesXml : BasicColorConfigurationXml
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
        [XmlAttribute]
        public RuleCheckType RuleCheckType { get; set; }

        internal bool Accepts(BuildingInfo info)
        {
            switch (RuleCheckType)
            {
                case RuleCheckType.ITEM_CLASS:
                    return info.m_class.name == ItemClassName;
                case RuleCheckType.SERVICE:
                    return info.m_class.m_service == Service;
                case RuleCheckType.SERVICE_SUBSERVICE:
                    return info.m_class.m_service == Service && info.m_class.m_subService == SubService;
                case RuleCheckType.SERVICE_LEVEL:
                    return info.m_class.m_service == Service && info.m_class.m_level == Level;
                case RuleCheckType.ASSET_NAME:
                    return info.name == AssetName;
            }
            return false;
        }
    }
}