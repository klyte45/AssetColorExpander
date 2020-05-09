using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class PropAssetFolderRuleXml : BasicColorConfigurationXml
    {
        [XmlAttribute(AttributeName = "assetName")]
        public string AssetName { get; set; }
    }
}