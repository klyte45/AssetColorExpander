using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.AssetColorExpander.UI
{
    public class ACEPanel : BasicKPanel<AssetColorExpanderMod, ACEController, ACEPanel>
    {
        public override float PanelWidth => GetComponentInParent<UIComponent>().width;

        public override float PanelHeight => GetComponentInParent<UIComponent>().height;
        public ACEBuildingRulesList RuleList { get; private set; }

        protected override void AwakeActions()
        {
            KlyteMonoUtils.CreateUIElement(out UIPanel secondaryContainer, MainPanel.transform, "SecContainer", new Vector4(0, 40, MainPanel.width, MainPanel.height - 40));
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