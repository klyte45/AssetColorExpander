using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.XML
{
    [XmlRoot(ElementName = "bceConfig")]
    public class BCEConfig<D> : ILibableAsContainer<D> where D : BasicColorConfigurationXml, new()
    {
        [XmlAttribute("rulesetName")]
        public override string SaveName { get; set; }
    }
}
