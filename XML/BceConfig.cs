using System.Collections.Generic;
using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.XML
{
    [XmlRoot(ElementName = "bceConfig")]
    public class BceConfig
    {
        [XmlElement(ElementName = "bce")]
        public List<ColorConfigurationXml> ConfigList { get; set; }
    }
}
