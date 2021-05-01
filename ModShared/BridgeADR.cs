extern alias ADR;
using ADR::Klyte.Addresses.ModShared;
using UnityEngine;

namespace Klyte.AssetColorExpander.ModShared
{
    internal class BridgeADR : MonoBehaviour, IBridgeADR
    {
        protected void Start() => AdrFacade.Instance.EventDistrictChanged += () => AssetColorExpanderMod.Controller.CleanCache();
        public bool AddressesAvailable { get; } = true;
        public Color GetDistrictColor(ushort districtId) => AdrFacade.GetDistrictColor(districtId);
    }
}

