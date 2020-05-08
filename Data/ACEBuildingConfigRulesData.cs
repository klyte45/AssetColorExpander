using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Data
{

    public class ACEBuildingConfigRulesData : DataExtensorBase<ACEBuildingConfigRulesData>
    {
        [XmlElement("rules")]
        public ACEBuildingConfig<BuildingCityDataRuleXml> Rules { get; private set; } = new ACEBuildingConfig<BuildingCityDataRuleXml>();
        public override string SaveId => "K45_ACE_ACEConfigRulesData";

    }

}
