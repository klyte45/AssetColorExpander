using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Data
{

    public class ACEVehicleConfigRulesData : DataExtensionBase<ACEVehicleConfigRulesData>
    {
        [XmlElement("rules")]
        public ACERulesetContainer<VehicleCityDataRuleXml> Rules { get; private set; } = new ACERulesetContainer<VehicleCityDataRuleXml>();
        public override string SaveId => "K45_ACE_ACEVehicleConfigRulesData";

    }

}
