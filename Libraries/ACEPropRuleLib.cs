using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("PropRuleLib")]
    public class ACEPropRuleLib : LibBaseFile<ACEPropRuleLib, PropCityDataRuleXml>
    {
        protected override string XmlName => "ACEPropRuleLib";
    }

}
