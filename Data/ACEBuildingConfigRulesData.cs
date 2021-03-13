using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Data
{

    public class ACEBuildingConfigRulesData : DataExtensorBase<ACEBuildingConfigRulesData>
    {
        [XmlElement("rules")]
        public ACERulesetContainer<BuildingCityDataRuleXml> Rules { get; private set; } = new ACERulesetContainer<BuildingCityDataRuleXml>();
        public override string SaveId => "K45_ACE_ACEBuildingConfigRulesData";

    }

}
