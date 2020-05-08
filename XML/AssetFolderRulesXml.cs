using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.XML
{
    public class AssetFolderRulesXml : BasicColorConfigurationXml
    {
        [XmlAttribute(AttributeName = "assetName")]
        public string AssetName { get; set; }
    }
}