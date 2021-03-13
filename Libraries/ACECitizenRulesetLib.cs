using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("CitizenRulesetLib")]
    public class ACECitizenRulesetLib : LibBaseFile<ACECitizenRulesetLib, ACERulesetContainer<CitizenCityDataRuleXml>>
    {
        protected override string XmlName => "ACECitizenRulesetLib";
    }

}
