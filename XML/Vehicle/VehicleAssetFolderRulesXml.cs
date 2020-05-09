using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{

    public class VehicleAssetFolderRuleXml : BasicVehicleColorConfigurationXml
    {
        [XmlAttribute(AttributeName = "assetName")]
        public string AssetName { get; set; }

    }
}
