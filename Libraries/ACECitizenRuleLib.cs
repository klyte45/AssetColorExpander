using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("CitizenRuleLib")]
    public class ACECitizenRuleLib : LibBaseFile<ACECitizenRuleLib, CitizenCityDataRuleXml>
    {
        protected override string XmlName => "ACECitizenRuleLib";
    }

}
