
using UnityEngine;

namespace Klyte.AssetColorExpander.ModShared
{
    internal interface IBridgeADR
    {
        public abstract bool AddressesAvailable { get; }
        public abstract Color GetDistrictColor(ushort districtId);

    }
}