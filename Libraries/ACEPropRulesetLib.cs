using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("PropRulesetLib")]
    public class ACEPropRulesetLib : LibBaseFile<ACEPropRulesetLib, ACERulesetContainer<PropCityDataRuleXml>>
    {
        protected override string XmlName => "ACEPropRulesetLib";
    }

}
