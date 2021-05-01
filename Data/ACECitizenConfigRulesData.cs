using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Data
{

    public class ACECitizenConfigRulesData : DataExtensionBase<ACECitizenConfigRulesData>
    {
        [XmlElement("rules")]
        public ACERulesetContainer<CitizenCityDataRuleXml> Rules { get; private set; } = new ACERulesetContainer<CitizenCityDataRuleXml>();
        public override string SaveId => "K45_ACE_ACECitizenConfigRulesData";

    }

}
