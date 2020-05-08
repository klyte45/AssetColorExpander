using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.Libraries
{
    [XmlRoot("RulesetConfigLib")]
    public class BCERulesetConfigLib : LibBaseFile<BCERulesetConfigLib, BCEConfig<CityDataRuleXml>>
    {
        protected override string XmlName => "BCERulesetConfigLib";
    }

}
