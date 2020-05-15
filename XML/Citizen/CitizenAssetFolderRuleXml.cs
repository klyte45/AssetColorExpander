using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class CitizenAssetFolderRuleXml : BasicColorConfigurationXml, IAssetNameable, IRuleCacheSource
    {
        [XmlAttribute]
        public string AssetName { get; set; }
        public RuleSource Source { get; set; }
    }
}