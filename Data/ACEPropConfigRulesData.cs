using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Data
{

    public class ACEPropConfigRulesData : DataExtensionBase<ACEPropConfigRulesData>
    {
        [XmlElement("rules")]
        public ACERulesetContainer<PropCityDataRuleXml> Rules { get; private set; } = new ACERulesetContainer<PropCityDataRuleXml>();
        public override string SaveId => "K45_ACE_ACEPropConfigRulesData";

    }

}
