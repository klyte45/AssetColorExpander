using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("BuildingRulesetLib")]
    public class ACEBuildingRulesetLib : LibBaseFile<ACEBuildingRulesetLib, ACEBuildingConfig<BuildingCityDataRuleXml>>
    {
        protected override string XmlName => "ACEBuildingRulesetLib";
    }

}
