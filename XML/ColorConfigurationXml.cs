using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.BuildingColorExpander.XML
{

    [XmlRoot(ElementName = "bce")]
    public class ColorConfigurationXml
    {
        [XmlElement(ElementName = "color")]
        public List<string> ColorListStr { get; set; }
        [XmlAttribute(AttributeName = "assetName")]
        public string AssetName { get; set; }
        [XmlAttribute(AttributeName = "coloringMode")]
        public ColoringMode ColoringMode { get; set; }
        [XmlAttribute(AttributeName = "pastelConfig")]
        public PastelConfig PastelConfig { get; set; } = PastelConfig.ALLOW_ALL;

        [XmlIgnore]
        public List<Color32> ColorList
        {
            get {

                if (m_colorList == null)
                {
                    m_colorList = ColorListStr?.Select(x => ColorExtensions.FromRGB(x))?.ToList() ?? new List<Color32>();
                }
                return m_colorList;
            }
        }

        [XmlIgnore]
        private List<Color32> m_colorList;

    }
}
