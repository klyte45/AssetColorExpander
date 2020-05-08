using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.AssetColorExpander.Libraries
{
    [XmlRoot("BuildingRuleLib")]
    public class ACEBuildingRuleLib : LibBaseFile<ACEBuildingRuleLib, BuildingCityDataRuleXml>
    {
        protected override string XmlName => "ACEBuildingRuleLib";
    }

}
