using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    [XmlRoot("VehicleColorConfig")]
    public class VehicleAssetFolderRuleXml : BasicVehicleColorConfigurationXml, IAssetNameable, IRuleCacheSource
    {
        [XmlAttribute(AttributeName = "assetName")]
        public string LegacyAssetName { set => AssetName = value; }
        [XmlAttribute]
        public string AssetName { get; set; }
        [XmlAttribute]
        public string BuildingName { get; set; }
        [XmlIgnore]
        public RuleSource Source { get; set; }

    }
}
