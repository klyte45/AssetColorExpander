using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("VehicleRulesetLib")]
    public class ACEVehicleRulesetLib : LibBaseFile<ACEVehicleRulesetLib, ACERulesetContainer<VehicleCityDataRuleXml>>
    {
        protected override string XmlName => "ACEVehicleRulesetLib";
    }

}
