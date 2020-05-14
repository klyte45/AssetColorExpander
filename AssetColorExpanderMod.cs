using ColossalFramework.Globalization;
using Klyte.AssetColorExpander.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[assembly: AssemblyVersion("0.0.0.*")]
namespace Klyte.AssetColorExpander
{
    public class AssetColorExpanderMod : BasicIUserMod<AssetColorExpanderMod, ACEController, ACEPanel>
    {

        public override string SimpleName => "Asset Color Expander";

        public override string Description => "Expand the color variation of assets by type";

        private UIHelperExtension m_groupListing;
        public override void TopSettingsUI(UIHelperExtension helper)
        {
            m_groupListing = helper.AddGroupExtended(Locale.Get("K45_ACE_CONFIG_LOADEDCUSTOMCONFIGREPORT_TITLE"));
            m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_BUILDINGS"), () => ShowModalReport(ACEController.CacheOrder.BUILDING));
            m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_CITIZENS"), () => ShowModalReport(ACEController.CacheOrder.CITIZEN));
            m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_NETWORKS"), () => ShowModalReport(ACEController.CacheOrder.NET));
            m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_PROPS"), () => ShowModalReport(ACEController.CacheOrder.PROP_PLACED));
            m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_VEHICLES"), () => ShowModalReport(ACEController.CacheOrder.VEHICLE));
            m_groupListing.Self.parent.isVisible = false;

        }

        protected override void OnLevelLoadingInternal()
        {
            base.OnLevelLoadingInternal();
            m_groupListing.Self.parent.isVisible = Controller != null;
        }

        private int itemsPerReportPage = 30;

        private void ShowModalReport(ACEController.CacheOrder target, int[] cachedStarts = null, int currentPage = 0)
        {
            ACEController.FormattedReportLine[] reference = Controller.GetLoadedReport(target);
            if (cachedStarts == null)
            {
                var itemStarts = reference.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Level == 0).Select(x => x.First).ToList();
                var startCacheBuilder = new List<int> { 0 };
                for (int i = 0; i < itemStarts.Count - 1; i++)
                {
                    if (itemStarts[i + 1] - startCacheBuilder.Last() > itemsPerReportPage)
                    {
                        startCacheBuilder.Add(itemStarts[i]);
                    }
                }
                if (reference.Length - startCacheBuilder.Last() > itemsPerReportPage)
                {
                    startCacheBuilder.Add(itemStarts.Last());
                }
                cachedStarts = startCacheBuilder.ToArray();
            }

            int firstItem = cachedStarts[currentPage];
            int lastItem = currentPage + 1 >= cachedStarts.Length ? reference.Length - 1 : cachedStarts[currentPage + 1] - 1;
            K45DialogControl.ShowModal(new K45DialogControl.BindProperties
            {
                title = string.Format(Locale.Get("K45_ACE_REPORTTITLEFORMAT", target.ToString()), currentPage + 1, cachedStarts.Length),
                message = lastItem < 0 ? Locale.Get("K45_ACE_NOITEMSLOADED") : string.Join("\n", reference.Where((x, y) => y >= firstItem && y <= lastItem).Select(x => x.ToString()).ToArray()),
                showClose = true,
                showButton1 = currentPage > 0,
                textButton1 = "<<<\n" + Locale.Get("K45_CMNS_PREV"),
                showButton2 = true,
                textButton2 = Locale.Get("K45_CMNS_OK"),
                showButton3 = currentPage < cachedStarts.Length - 1,
                textButton3 = ">>>\n" + Locale.Get("K45_CMNS_NEXT")

            }, (x) =>
            {
                if (x == 1)
                {
                    ShowModalReport(target, cachedStarts, currentPage - 1);
                }
                if (x == 3)
                {
                    ShowModalReport(target, cachedStarts, currentPage + 1);
                }
                return true;
            });

        }


        //private static void AddFolderButton(string filePath, UIHelperExtension helper, string localeId)
        //{
        //    FileInfo fileInfo = FileUtils.EnsureFolderCreation(filePath);
        //    helper.AddLabel(Locale.Get(localeId) + ":");
        //    var namesFilesButton = ((UIButton)helper.AddButton("/", () => ColossalFramework.Utils.OpenInFileBrowser(fileInfo.FullName)));
        //    namesFilesButton.textColor = Color.yellow;
        //    KlyteMonoUtils.LimitWidth(namesFilesButton, 710);
        //    namesFilesButton.text = fileInfo.FullName + Path.DirectorySeparatorChar;
        //}       

    }
}