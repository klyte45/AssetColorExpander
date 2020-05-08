using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.BuildingColorExpander.Data;
using Klyte.BuildingColorExpander.Libraries;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.BuildingColorExpander.UI
{

    internal class BCERuleEditor : UICustomControl
    {
        public UIPanel MainContainer { get; protected set; }

        private Dictionary<string, string> m_buildingsLoaded;

        private const string DISTRICT_SELECTOR_TEMPLATE = "K45_BCE_DistrictSelectorTemplate";
        private int m_currentIdx = -1;

        private UITabstrip m_tabstrip;

        private UITextField m_name;

        private UIButton m_copySettings;
        private UIButton m_pasteSettings;

        private UIDropDown m_ruleFilter;
        private UIDropDown m_service;
        private UIDropDown m_subService;
        private UIDropDown m_level;
        private UIDropDown m_class;
        private UITextField m_assetFilter;
        private UIListBox m_popup;

        private UICheckBox m_districtWhiteList;
        private UICheckBox m_districtBlackList;
        private UIDropDown m_districtResolutionOrder;
        private UIPanel m_listContainer;
        private UIScrollablePanel m_districtList;
        private UITemplateList<UIPanel> m_checkboxTemplateList;



        public void Awake()
        {
            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.clipChildren = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(0, 0, 4, 4);

            KlyteMonoUtils.CreateTabsComponent(out m_tabstrip, out UITabContainer m_tabContainer, MainContainer.transform, "TextEditor", new Vector4(0, 0, MainContainer.width, 40), new Vector4(0, 0, MainContainer.width, MainContainer.height - 40));
            UIPanel m_tabSettings = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Settings), "K45_BCE_BUILDINGRULES_BASIC_SETTINGS", "RcSettings");
            UIPanel m_tabAppearence = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoColorIcon), "K45_BCE_BUILDINGRULES_APPEARANCE_SETTINGS", "RcAppearence");
            UIPanel m_tabDistricts = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, "ToolbarIconDistrict", "K45_BCE_BUILDINGRULES_DISTRICT_SETTINGS", "RcDistricts");
            UIPanel m_tabLib = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Load), "K45_BCE_BUILDINGRULES_LIB_SETTINGS", "RcLib");

            var helperSettings = new UIHelperExtension(m_tabSettings, LayoutDirection.Vertical);
            var helperAppearence = new UIHelperExtension(m_tabAppearence, LayoutDirection.Vertical);
            var helperDistricts = new UIHelperExtension(m_tabDistricts, LayoutDirection.Vertical);
            var helperLib = new UIHelperExtension(m_tabLib, LayoutDirection.Vertical);


            AddTextField(Locale.Get("K45_BCE_BUILDINGRULES_NAME"), out m_name, helperSettings, OnSetName);
            helperSettings.AddSpace(5);

            AddDropdown(Locale.Get("K45_BCE_BUILDINGRULES_RULEFILTER"), out m_ruleFilter, helperSettings, Enum.GetNames(typeof(RuleCheckType)).Select(x => Locale.Get("K45_BCE_RULECHECKTYPE", x)).ToArray(), OnChangeRuleCheckType);
            AddDropdown(Locale.Get("K45_BCE_BUILDINGRULES_SERVICEFILTER"), out m_service, helperSettings, Enum.GetNames(typeof(ItemClass.Service)).Select(x => $"{x}").ToArray(), OnChangeServiceFilter);
            AddDropdown(Locale.Get("K45_BCE_BUILDINGRULES_SUBSERVICEFILTER"), out m_subService, helperSettings, Enum.GetNames(typeof(ItemClass.SubService)).Select(x => $"{x}").ToArray(), OnChangeSubServiceFilter);
            AddDropdown(Locale.Get("K45_BCE_BUILDINGRULES_LEVELFILTER"), out m_level, helperSettings, (Enum.GetValues(typeof(ItemClass.Level)) as ItemClass.Level[]).OrderBy(x => (int)x).Select(x => $"{x}").ToArray(), OnChangeLevelFilter);
            AddDropdown(Locale.Get("K45_BCE_BUILDINGRULES_CLASSFILTER"), out m_class, helperSettings, new string[0], OnChangeClassFilter);
            AddTextField(Locale.Get("K45_BCE_BUILDINGRULES_ASSETSELECT"), out m_assetFilter, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilter);
            m_popup = ConfigureListSelectionPopupForUITextField(m_assetFilter, FilterBuildingByText, OnAssetSelectedChanged, GetCurrentSelectionName);

            AddLibBox<BCEConfigLib, CityDataRuleXml>(helperLib, out m_copySettings, OnCopyRule, out m_pasteSettings, OnPasteRule, out _, null, OnLoadRule, GetRuleSerialized);

            AddCheckboxLocale("K45_BCE_BUILDINGRULES_DISTRICTSELECTIONASWHITELIST", out m_districtWhiteList, helperDistricts, OnSetDistrictsAsWhitelist);
            AddCheckboxLocale("K45_BCE_BUILDINGRULES_DISTRICTSELECTIONASBLACKLIST", out m_districtBlackList, helperDistricts, OnSetDistrictsAsBlacklist);
            AddDropdown(Locale.Get("K45_BCE_BUILDINGRULES_DISTRICTRESTRICTIONSOLVEORDER"), out m_districtResolutionOrder, helperDistricts, Enum.GetNames(typeof(DistrictRestrictionOrder)).Select(x => Locale.Get("K45_BCE_DISTRICTRESTRICTIONORDER", x)).ToArray(), OnChangeDistrictRestrictionOrder);
            KlyteMonoUtils.CreateUIElement(out m_listContainer, helperDistricts.Self.transform, "previewPanel", new UnityEngine.Vector4(0, 0, helperDistricts.Self.width, helperDistricts.Self.height - 160));
            KlyteMonoUtils.CreateScrollPanel(m_listContainer, out m_districtList, out _, m_listContainer.width - 20, m_listContainer.height);
            m_districtList.backgroundSprite = "OptionsScrollbarTrack";
            m_districtList.autoLayout = true;
            m_districtList.autoLayoutDirection = LayoutDirection.Vertical;

            CreateTemplateDistrict();
            m_checkboxTemplateList = new UITemplateList<UIPanel>(m_districtList, DISTRICT_SELECTOR_TEMPLATE);


            BCEPanel.Instance.RuleList.EventSelectionChanged += OnChangeTab;
            MainContainer.isVisible = false;
            m_pasteSettings.isVisible = false;
        }

        public void Start() => m_class.items = BuildingColorExpanderMod.Controller?.AllClassesBuilding?.Keys?.Select(x => x.name)?.OrderBy(x => x)?.ToArray() ?? new string[0];

        #region Prefab handling
        public Dictionary<string, string> BuildingsLoaded
        {
            get {
                if (m_buildingsLoaded == null)
                {
                    m_buildingsLoaded = GetInfos<BuildingInfo>().Where(x => x != null).ToDictionary(x => GetListName(x), x => x?.name);
                }
                return m_buildingsLoaded;
            }
        }
        private static string GetListName(PrefabInfo x) => (x?.name?.EndsWith("_Data") ?? false) ? $"{x?.GetLocalizedTitle()}" : x?.name ?? "";
        private List<T> GetInfos<T>() where T : PrefabInfo
        {
            var list = new List<T>();
            uint num = 0u;
            while (num < (ulong)PrefabCollection<T>.LoadedCount())
            {
                T prefabInfo = PrefabCollection<T>.GetLoaded(num);
                if (prefabInfo != null)
                {
                    list.Add(prefabInfo);
                }
                num += 1u;
            }
            return list;
        }

        private string[] FilterBuildingByText() => BuildingsLoaded
            .ToList()
            .Where((x) => m_assetFilter.text.IsNullOrWhiteSpace() ? true : LocaleManager.cultureInfo.CompareInfo.IndexOf(x.Value + (PrefabUtils.instance.AuthorList.TryGetValue(x.Value.Split('.')[0], out string author) ? "\n" + author : ""), m_assetFilter.text, CompareOptions.IgnoreCase) >= 0)
            .Select(x => x.Key)
            .OrderBy((x) => x)
            .ToArray();

        private string GetCurrentSelectionName()
        {
            string name = "";
            SafeObtain((ref CityDataRuleXml x) => name = x.AssetName);
            return name ?? "";
        }
        #endregion

        #region District
        private void UpdateDistrictList(ref CityDataRuleXml reference)
        {
            var districts = DistrictUtils.GetValidParks().ToDictionary(x => x.Key, x => 0x100 | x.Value).Union(DistrictUtils.GetValidDistricts()).OrderBy(x => x.Value == 0 ? "" : x.Key).ToDictionary(x => x.Key, x => x.Value);
            ref DistrictPark[] parkBuffer = ref Singleton<DistrictManager>.instance.m_parks.m_buffer;
            UIPanel[] districtChecks = m_checkboxTemplateList.SetItemCount(districts.Count);

            for (int i = 0; i < districts.Count; i++)
            {
                string districtName = districts.Keys.ElementAt(i);
                UICheckBox checkbox = districtChecks[i].GetComponentInChildren<UICheckBox>();
                checkbox.stringUserData = districts[districtName].ToString();
                if (checkbox.label.objectUserData == null)
                {
                    checkbox.eventCheckChanged += (x, y) =>
                    {
                        SafeObtain((ref CityDataRuleXml z) =>
                        {
                            if (ushort.TryParse(x.stringUserData, out ushort districtIdx))
                            {
                                if (y)
                                {
                                    z.SelectedDistricts.Add(districtIdx);
                                }
                                else
                                {
                                    z.SelectedDistricts.Remove(districtIdx);
                                }
                            }
                        });

                    };
                    KlyteMonoUtils.LimitWidthAndBox(checkbox.label, m_districtList.width - 50);


                    checkbox.label.objectUserData = true;
                }
                checkbox.text = districtName;
                if (districts[districtName] >= 256)
                {
                    int parkId = districts[districtName] & 0xFF;
                    if (parkBuffer[parkId].IsCampus)
                    {
                        checkbox.tooltip = Locale.Get("MAIN_TOOL", "PlayerEducation");
                        checkbox.label.textColor = Color.cyan;
                    }
                    else if (parkBuffer[parkId].IsIndustry)
                    {
                        checkbox.tooltip = Locale.Get("PARKSOVERVIEW_TOOLTIP", "Industry");
                        checkbox.label.textColor = Color.yellow;
                    }
                    else if (parkBuffer[parkId].IsPark)
                    {
                        checkbox.tooltip = Locale.Get("PARKSOVERVIEW_TOOLTIP", "Generic");
                        checkbox.label.textColor = Color.green;
                    }
                    else
                    {
                        checkbox.tooltip = Locale.Get("MAIN_AREAS");
                        checkbox.label.textColor = Color.Lerp(Color.magenta, Color.blue, 0.5f);
                    }
                }
                else
                {
                    checkbox.tooltip = Locale.Get("TUTORIAL_ADVISER_TITLE", "District");
                    checkbox.label.textColor = Color.white;
                }

            }
            for (int i = 0; i < m_checkboxTemplateList.items.Count; i++)
            {
                UICheckBox checkbox = m_checkboxTemplateList.items[i].GetComponentInChildren<UICheckBox>();
                if (ushort.TryParse(checkbox.stringUserData, out ushort districtIdx))
                {
                    checkbox.isChecked = reference.SelectedDistricts.Contains(districtIdx);
                }
            }
        }

        private void CreateTemplateDistrict()
        {
            if (!UITemplateUtils.GetTemplateDict().ContainsKey(DISTRICT_SELECTOR_TEMPLATE))
            {
                var go = new GameObject();
                UIPanel panel = go.AddComponent<UIPanel>();
                panel.size = new Vector2(m_districtList.width, 36);
                panel.autoLayout = true;
                panel.wrapLayout = false;
                panel.autoLayoutDirection = LayoutDirection.Horizontal;

                UICheckBox uiCheckbox = UIHelperExtension.AddCheckbox(panel, "AAAAAA", false);
                uiCheckbox.name = "AssetCheckbox";
                uiCheckbox.height = 29f;
                uiCheckbox.width = 290f;
                uiCheckbox.label.processMarkup = true;
                uiCheckbox.label.textScale = 0.8f;

                UITemplateUtils.GetTemplateDict()[DISTRICT_SELECTOR_TEMPLATE] = panel;
            }
        }
        #endregion

        private delegate void SafeObtainMethod(ref CityDataRuleXml x);
        private void SafeObtain(SafeObtainMethod action, int? targetTab = null)
        {
            int effTargetTab = Math.Max(-1, targetTab ?? m_currentIdx);
            if (effTargetTab < 0)
            {
                return;
            }

            if (effTargetTab < BCEConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                action(ref BCEConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
                BuildingColorExpanderMod.Controller.CleanCache();
            }
        }
        private void OnChangeTab(int obj)
        {
            MainContainer.isVisible = obj >= 0;
            m_currentIdx = obj;
            ReloadData();
        }

        private void ReloadData()
        {
            SafeObtain((ref CityDataRuleXml x) =>
            {
                m_name.text = x.SaveName;

                m_districtWhiteList.isChecked = !x.SelectedDistrictsIsBlacklist;
                m_districtBlackList.isChecked = x.SelectedDistrictsIsBlacklist;
                m_districtResolutionOrder.selectedIndex = (int)x.DistrictRestrictionOrder;

                m_ruleFilter.selectedIndex = (int)x.RuleCheckType;

                m_service.selectedIndex = (int)x.Service;
                m_subService.selectedIndex = (int)x.SubService;
                m_level.selectedIndex = (int)x.Level;
                m_class.selectedValue = x.ItemClassName;

                string targetAsset = x.AssetName ?? "";
                m_lastSelection = GetInfos<BuildingInfo>().Where(y => y.name == targetAsset).FirstOrDefault();
                m_assetFilter.text = GetListName(m_lastSelection);

                ApplyRuleCheck(x);

                UpdateDistrictList(ref x);
            });
        }

        private void ApplyRuleCheck(CityDataRuleXml x)
        {
            m_service.parent.isVisible = x.RuleCheckType == RuleCheckType.SERVICE || x.RuleCheckType == RuleCheckType.SERVICE_LEVEL || x.RuleCheckType == RuleCheckType.SERVICE_SUBSERVICE;
            m_subService.parent.isVisible = x.RuleCheckType == RuleCheckType.SERVICE_SUBSERVICE;
            m_level.parent.isVisible = x.RuleCheckType == RuleCheckType.SERVICE_LEVEL;
            m_class.parent.isVisible = x.RuleCheckType == RuleCheckType.ITEM_CLASS;
            m_assetFilter.parent.isVisible = x.RuleCheckType == RuleCheckType.ASSET_NAME;
        }

        private string m_clipboard;
        private BuildingInfo m_lastSelection;

        private string GetRuleSerialized()
        {
            int effTargetTab = Math.Max(-1, m_currentIdx);
            if (effTargetTab >= 0 && effTargetTab < BCEConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                return XmlUtils.DefaultXmlSerialize(BCEConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
            }
            else
            {
                return null;
            }
        }

        private void OnLoadRule(string obj) => SafeObtain((ref CityDataRuleXml x) =>
        {
            x = XmlUtils.DefaultXmlDeserialize<CityDataRuleXml>(obj);
            BCEPanel.Instance.RuleList.FixTabstrip();
            ReloadData();
        });
        private void OnPasteRule() => OnLoadRule(m_clipboard);
        private void OnCopyRule() => SafeObtain((ref CityDataRuleXml x) =>
        {
            m_clipboard = XmlUtils.DefaultXmlSerialize(x);
            m_pasteSettings.isVisible = true;
        });

        private void OnSetDistrictsAsBlacklist(bool isChecked) => SafeObtain((ref CityDataRuleXml x) => { x.SelectedDistrictsIsBlacklist = isChecked; m_districtWhiteList.isChecked = !isChecked; });
        private void OnSetDistrictsAsWhitelist(bool isChecked) => SafeObtain((ref CityDataRuleXml x) => { x.SelectedDistrictsIsBlacklist = !isChecked; m_districtBlackList.isChecked = !isChecked; });
        private void OnChangeDistrictRestrictionOrder(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.DistrictRestrictionOrder = (DistrictRestrictionOrder)sel;
            }
        });

        private void OnSetName(string text) => SafeObtain((ref CityDataRuleXml x) =>
        {
            if (!text.IsNullOrWhiteSpace())
            {
                x.SaveName = text;
                BCEPanel.Instance.RuleList.FixTabstrip();
                OnChangeTab(m_currentIdx);
            }
            else
            {
                m_name.text = x.SaveName;
            }
        });

        private void OnChangeRuleCheckType(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            x.RuleCheckType = (RuleCheckType)sel;
            ApplyRuleCheck(x);
        });


        private void OnAssetSelectedChanged(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            BuildingInfo k = (sel < 0 ? null : m_lastSelection = GetInfos<BuildingInfo>().Where(x => x.name == BuildingsLoaded[m_popup.items[sel]]).FirstOrDefault());
            x.AssetName = k.name;
        });
        private void OnChangeClassFilter(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ItemClassName = m_class.items[sel];
            }
        });
        private void OnChangeLevelFilter(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Level = (ItemClass.Level)sel - 1;
            }
        });
        private void OnChangeSubServiceFilter(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.SubService = (ItemClass.SubService)sel;
            }
        });
        private void OnChangeServiceFilter(int sel) => SafeObtain((ref CityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Service = (ItemClass.Service)sel;
            }
        });
    }

}
