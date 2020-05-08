using ColossalFramework.UI;
using Klyte.BuildingColorExpander.Data;
using Klyte.BuildingColorExpander.Libraries;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Interfaces;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.BuildingColorExpander.UI
{
    public class BCEPanel : BasicKPanel<BuildingColorExpanderMod, BCEController, BCEPanel>
    {
        public override float PanelWidth => GetComponentInParent<UIComponent>().width;

        public override float PanelHeight => GetComponentInParent<UIComponent>().height;
        public BCERulesList RuleList { get; private set; }

        protected override void AwakeActions()
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel secondaryContainer, MainPanel.transform, "SecContainer", new Vector4(0, 40, MainPanel.width, MainPanel.height - 40));
            secondaryContainer.autoLayout = true;
            secondaryContainer.autoLayoutDirection = LayoutDirection.Horizontal;
            secondaryContainer.autoLayoutPadding = new RectOffset(0, 10, 0, 0);

            KlyteMonoUtils.CreateUIElement(out UIPanel tertiaryContainer, secondaryContainer.transform, "TrcContainer", new Vector4(0, 0, secondaryContainer.width * 0.25f, secondaryContainer.height));
            RuleList = tertiaryContainer.gameObject.AddComponent<BCERulesList>();

            KlyteMonoUtils.CreateUIElement(out UIPanel editorPanel, secondaryContainer.transform, "EditPanel", new Vector4(0, 0, secondaryContainer.width * 0.75f - 35, secondaryContainer.height));
            editorPanel.gameObject.AddComponent<WTSRoadCornerEditorDetailTabs>();
        }
    }

    public class BCERulesList : BasicRulesList<CityDataRulesXml, BCERulesetConfigLib, BCEConfig<CityDataRulesXml>>
    {
        protected override ref CityDataRulesXml[] ReferenceData => ref BCEConfigRulesData.Instance.Rules.m_dataArray;

        protected override string LocaleRuleListTitle => "K45_BCE_RULELISTTITLE";

        protected override string LocaleImport => "K45_BCE_IMPORTRULELIST";

        protected override string LocaleExport => "K45_BCE_EXPORTRULELIST";

        protected override void Help_RulesList() { }
        protected override void OnTabstripFix() => BuildingColorExpanderMod.Controller?.CleanCache();
    }
}