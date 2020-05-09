using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.AssetColorExpander.UI
{
    public class ACEPanel : BasicKPanel<AssetColorExpanderMod, ACEController, ACEPanel>
    {
        private UITabstrip m_stripMain;

        public override float PanelWidth => GetComponentInParent<UIComponent>().width;

        public override float PanelHeight => GetComponentInParent<UIComponent>().height + 40;

        public ACEBuildingController BuildingTab { get; private set; }
        public ACEVehicleController VehicleTab { get; private set; }

        protected override void AwakeActions()
        {
            KlyteMonoUtils.CreateUIElement(out m_stripMain, MainPanel.transform, "ACETabstrip", new Vector4(5, 40, MainPanel.width - 10, 40));
            m_stripMain.startSelectedIndex = -1;
            m_stripMain.selectedIndex = -1;

            KlyteMonoUtils.CreateUIElement(out UITabContainer tabContainer, MainPanel.transform, "ACETabContainer", new Vector4(0, 80, MainPanel.width, MainPanel.height - 90));
            m_stripMain.tabPages = tabContainer;

            BuildingTab = m_stripMain.CreateTabLocalized<ACEBuildingController>("IconAssetBuilding", "K45_ACE_BUILDINGSRULEEDITOR_TAB", "ACEBuildingEditorTab", false);
            VehicleTab = m_stripMain.CreateTabLocalized<ACEVehicleController>("IconAssetVehicle", "K45_ACE_VEHICLESRULEEDITOR_TAB", "ACEVehicleEditorTab", false);
        }
    }
}