using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Libraries;
using System.Xml.Serialization;

namespace Klyte.BuildingColorExpander.Libraries
{
    [XmlRoot("ConfigLib")]
    public class BCEConfigLib : LibBaseFile<BCEConfigLib, CityDataRulesXml>
    {
        protected override string XmlName => "BCEConfigLib";
    }

}
