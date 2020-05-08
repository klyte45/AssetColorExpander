using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Interfaces;
using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.Data
{

    public class BCEConfigRulesData : DataExtensorBase<BCEConfigRulesData>
    {
        [XmlElement("rules")]
        public BCEConfig<CityDataRulesXml> Rules { get; private set; } = new BCEConfig<CityDataRulesXml>();
        public override string SaveId => "K45_BCE_BCEConfigRulesData";

    }

}
