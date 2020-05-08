using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;

namespace Klyte.BuildingColorExpander.XML
{

    [XmlRoot(ElementName = "bce")]
    public abstract class BasicColorConfigurationXml : ILibable
    {
        [XmlElement(ElementName = "color")]
        public string[] ColorListStr
        {
            get => m_colorList?.Select(x => x.ToRGB()).ToArray();
            set => m_colorList = value?.Select(x => ColorExtensions.FromRGB(x))?.ToList() ?? new List<Color32>();
        }
        [XmlAttribute(AttributeName = "coloringMode")]
        public ColoringMode ColoringMode { get; set; }
        [XmlAttribute(AttributeName = "pastelConfig")]
        public PastelConfig PastelConfig { get; set; } = PastelConfig.ALLOW_ALL;

        [XmlAttribute("ruleName")]
        public string SaveName { get; set; }
        [XmlIgnore]
        internal List<Color32> m_colorList = new List<Color32>();

    }
}