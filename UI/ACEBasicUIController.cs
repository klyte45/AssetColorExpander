using ColossalFramework.UI;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Libraries;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using UnityEngine;

namespace Klyte.AssetColorExpander.UI
{
    public abstract class ACEBasicUIController<E, R, D, L, I> : UICustomControl where E : UICustomControl where R : BasicRulesList<D, L, I> where D : ILibable, new() where L : LibBaseFile<L, I>, new() where I : ILibableAsContainer<D>, new()
    {
        public R RuleList { get; private set; }
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
            RuleList = tertiaryContainer.gameObject.AddComponent<R>();

            KlyteMonoUtils.CreateUIElement(out UIPanel editorPanel, secondaryContainer.transform, "EditPanel", new Vector4(0, 0, secondaryContainer.width * 0.75f - 35, secondaryContainer.height));
            editorPanel.gameObject.AddComponent<E>();
        }
    }

}