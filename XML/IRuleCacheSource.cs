using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.XML
{
    public interface IRuleCacheSource
    {
        [XmlIgnore]
        RuleSource Source { get; set; }
    }
}