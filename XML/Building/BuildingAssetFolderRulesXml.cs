using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    [XmlRoot("BuildingColorConfig")]
    public class BuildingAssetFolderRuleXml : BasicColorConfigurationXml, IAssetNameable,IRuleCacheSource
    {
        [XmlAttribute("assetName")]
        public string LegacyAssetName { set => AssetName = value; }
        [XmlAttribute]
        public string AssetName { get; set; }
        public RuleSource Source { get; set; }
    }
}