using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public class ACERulesetContainer<D> : ILibableAsContainer<D> where D : BasicColorConfigurationXml, new()
    {
        [XmlAttribute("rulesetName")]
        public override string SaveName { get; set; }
    }
}
