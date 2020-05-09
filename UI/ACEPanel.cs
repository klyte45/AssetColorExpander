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

        protected override void AwakeActions()
        {
            KlyteMonoUtils.CreateUIElement(out m_stripMain, MainPanel.transform, "ACETabstrip", new Vector4(5, 40, MainPanel.width - 10, 40));
            m_stripMain.startSelectedIndex = -1;
            m_stripMain.selectedIndex = -1;

            KlyteMonoUtils.CreateUIElement(out UITabContainer tabContainer, MainPanel.transform, "ACETabContainer", new Vector4(0, 80, MainPanel.width, MainPanel.height - 90));
            m_stripMain.tabPages = tabContainer;

            //m_stripMain.CreateTabLocalized<WTSPropPlacingTab2>("InfoIconEscapeRoutes", "K45_WTS_HIGHWAY_SIGN_CONFIG_TAB", "WTSHighwaySign");
            //m_stripMain.CreateTabLocalized<WTSMileageMarkerTab3>("LocationMarkerNormal", "K45_WTS_MILEAGE_MARKERS_CONFIG_TAB", "WTSMileageMarkerTab");
            //m_stripMain.CreateTabLocalized<WTSBuildingEditorTab2>("IconAssetBuilding", "K45_WTS_BUILDING_CONFIG_TAB", "WTSBuildingEditorTab");
            BuildingTab = m_stripMain.CreateTabLocalized<ACEBuildingController>("IconAssetBuilding", "K45_ACE_BUILDINGSRULEEDITOR_TAB", "ACEBuildingEditorTab", false);
        }
    }

    public class ACEBuildingController : UICustomControl
    {
        public ACEBuildingRulesList RuleList { get; private set; }
        public UIPanel MainPanel { get; private set; }

        public void Awake()
        {
            MainPanel = GetComponent<UIPanel>();
            MainPanel.autoLayout = true;
            MainPanel.autoLayoutDirection = LayoutDirection.Vertical;
            MainPanel.autoLayoutPadding = new RectOffset(5, 5, 5, 5);

            KlyteMonoUtils.CreateUIElement(out UIPanel secondaryContainer, MainPanel.transform, "SecContainer", new Vector4(0, 0, MainPanel.width, MainPanel.height));
            secondaryContainer.autoLayout = true;
            secondaryContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            secondaryContainer.autoLayoutPadding = new RectOffset(0, 10, 0, 0);

            KlyteMonoUtils.CreateUIElement(out UIPanel tertiaryContainer, secondaryContainer.transform, "TrcContainer", new Vector4(0, 0, secondaryContainer.width * 0.25f, secondaryContainer.height));
            RuleList = tertiaryContainer.gameObject.AddComponent<ACEBuildingRulesList>();

            KlyteMonoUtils.CreateUIElement(out UIPanel editorPanel, secondaryContainer.transform, "EditPanel", new Vector4(0, 0, secondaryContainer.width * 0.75f - 35, secondaryContainer.height));
            editorPanel.gameObject.AddComponent<ACEBuildingRuleEditor>();
        }
    }
}