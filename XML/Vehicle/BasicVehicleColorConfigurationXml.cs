using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.AssetColorExpander.XML
{

    [XmlRoot(ElementName = "clrcnf")]
    public abstract class BasicVehicleColorConfigurationXml : BasicColorConfigurationXml
    {
        [XmlAttribute]
        public bool AllowDifferentColorsOnWagons { get; set; }
    }
}