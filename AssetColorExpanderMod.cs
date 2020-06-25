using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.AssetColorExpander.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Klyte.AssetColorExpander.ACELoadedDataContainer;

[assembly: AssemblyVersion("1.0.0.2")]
namespace Klyte.AssetColorExpander
{
    public class AssetColorExpanderMod : BasicIUserMod<AssetColorExpanderMod, ACEController, ACEPanel>
    {

        public override string SimpleName => "Asset Color Expander";

        public override string Description => "Expand the color variation of assets by type";

        private UIHelperExtension m_groupListing;
        internal UIButton m_building;
        internal UIButton m_citizen;
        internal UIButton m_net;
        internal UIButton m_prop;
        internal UIButton m_vehicle;
        public override void TopSettingsUI(UIHelperExtension helper)
        {
            Instance.m_groupListing = helper.AddGroupExtended(Locale.Get("K45_ACE_CONFIG_LOADEDCUSTOMCONFIGREPORT_TITLE"));
            Instance.m_building = Instance.m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_BUILDINGS"), () => ShowModalReport(ACEController.CacheOrder.BUILDING)) as UIButton;
            Instance.m_citizen = Instance.m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_CITIZENS"), () => ShowModalReport(ACEController.CacheOrder.CITIZEN)) as UIButton;
            Instance.m_net = Instance.m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_NETWORKS"), () => ShowModalReport(ACEController.CacheOrder.NET)) as UIButton;
            Instance.m_prop = Instance.m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_PROPS"), () => ShowModalReport(ACEController.CacheOrder.PROP_PLACED)) as UIButton;
            Instance.m_vehicle = Instance.m_groupListing.AddButton(Locale.Get("K45_ACE_CONFIG_LOADEDBUTTON_VEHICLES"), () => ShowModalReport(ACEController.CacheOrder.VEHICLE)) as UIButton;
            Instance.m_groupListing.Self.parent.isVisible = false;

        }

        protected override void OnLevelLoadingInternal()
        {
            base.OnLevelLoadingInternal();
            Instance.m_groupListing.Self.parent.isVisible = Controller != null;

        }

        private int m_itemsPerReportPage = 30;

        private void ShowModalReport(ACEController.CacheOrder target, int[] cachedStarts = null, int currentPage = 0)
        {
            FormattedReportLine[] reference = Controller.LoadedConfiguration.GetLoadedReport(target);
            if (cachedStarts == null)
            {
                var itemStarts = reference.Select((x, y) => Tuple.New(y, x)).Where(x => x.Second.Level == 0).Select(x => x.First).ToList();
                var startCacheBuilder = new List<int> { 0 };
                for (int i = 0; i < itemStarts.Count - 1; i++)
                {
                    if (itemStarts[i + 1] - startCacheBuilder.Last() > m_itemsPerReportPage)
                    {
                        startCacheBuilder.Add(itemStarts[i]);
                    }
                }
                if (reference.Length - startCacheBuilder.Last() > m_itemsPerReportPage)
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
    }
}