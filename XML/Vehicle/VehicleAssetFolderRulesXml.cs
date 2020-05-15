using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{

    public class VehicleAssetFolderRuleXml : BasicVehicleColorConfigurationXml, IAssetNameable, IRuleCacheSource
    {
        [XmlAttribute(AttributeName = "assetName")]
        public string LegacyAssetName { set => AssetName = value; }
        [XmlAttribute]
        public string AssetName { get; set; }
        [XmlAttribute]
        public string BuildingName { get; set; }
        public RuleSource Source { get; set; }

    }
}
