using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("VehcileRuleLib")]
    public class ACEVehcileRuleLib : LibBaseFile<ACEVehcileRuleLib, VehicleCityDataRuleXml>
    {
        protected override string XmlName => "ACEVehcileRuleLib";
    }

}
