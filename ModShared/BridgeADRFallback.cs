using UnityEngine;

namespace Klyte.AssetColorExpander.ModShared
{
    internal class BridgeADRFallback : MonoBehaviour, IBridgeADR
    {

        private readonly Color[] m_randomColors = { Color.black, Color.gray, Color.white, Color.red, new Color32(0xFF, 0x88, 0, 0xFf), Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta };

        public bool AddressesAvailable { get; } = false;

        public Color GetDistrictColor(ushort districtId) => m_randomColors[districtId % m_randomColors.Length];
    }
}

