using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class PropAssetFolderRuleXml : BasicColorConfigurationXml, IAssetNameable, IRuleCacheSource
    {
        [XmlAttribute]
        public string AssetName { get; set; }
        public RuleSource Source { get; set; }
        [XmlAttribute]
        internal string BuildingName { get; set; }
        [XmlAttribute]
        internal string NetName { get; set; }
    }
}