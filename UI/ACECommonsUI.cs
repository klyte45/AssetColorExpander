using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.AssetColorExpander.UI
{
    public static class ACECommonsUI
    {
        public static void GenerateExportButtons(UIHelperExtension helperSettings, string assetKind,
             out UIPanel m_exportButtonContainer, out UIButton m_exportButton, OnButtonClicked OnExport,
             out UIPanel m_exportButtonContainerLocal, out UIButton m_exportButtonLocal, OnButtonClicked OnExportLocal)
        {
            KlyteMonoUtils.CreateUIElement(out m_exportButtonContainer, helperSettings.Self.transform, $"ExportContainer{assetKind}", new Vector4(0, 0, helperSettings.Self.width, 45));
            m_exportButtonContainer.autoLayout = true;
            m_exportButtonContainer.autoLayoutPadding = new RectOffset(0, 6, 0, 0);
            m_exportButton = UIHelperExtension.AddButton(m_exportButtonContainer, Locale.Get($"K45_ACE_EXPORTDATA_TOASSET{assetKind.ToUpper()}"), OnExport);
            KlyteMonoUtils.LimitWidthAndBox(m_exportButton, m_exportButtonContainer.width * 0.7f);

            KlyteMonoUtils.CreateUIElement(out m_exportButtonContainerLocal, helperSettings.Self.transform, $"ExportContainerLocal{assetKind}", new Vector4(0, 0, helperSettings.Self.width, 45));
            m_exportButtonContainerLocal.autoLayout = true;
            m_exportButtonContainerLocal.autoLayoutPadding = new RectOffset(0, 6, 0, 0);
            m_exportButtonLocal = UIHelperExtension.AddButton(m_exportButtonContainerLocal, Locale.Get($"K45_ACE_EXPORTDATA_TOLOCAL{assetKind.ToUpper()}"), OnExportLocal);
            KlyteMonoUtils.LimitWidthAndBox(m_exportButtonLocal, m_exportButtonContainerLocal.width * 0.7f);
        }

    }

}
