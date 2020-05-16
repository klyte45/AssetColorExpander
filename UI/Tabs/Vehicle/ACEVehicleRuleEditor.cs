using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.Libraries;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensors;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using System.Linq;
using UnityEngine;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.AssetColorExpander.UI
{

    public class ACEVehicleRuleEditor : UICustomControl
    {
        public UIPanel MainContainer { get; protected set; }


        private const string COLOR_SELECTOR_TEMPLATE = "K45_ACE_ColorListSelectorTemplate";
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
        private UITextField m_assetFilterSelf;
        private UIListBox m_popupSelf;
        private UITextField m_assetFilterOwner;
        private UIListBox m_popupOwner;

        private UICheckBox m_allowWagonDifferentColors;
        private UIDropDown m_colorMode;
        private UIPanel m_listColorContainer;
        private UIScrollablePanel m_colorListScroll;
        private UITemplateList<UIPanel> m_colorFieldTemplateListColors;
        private UIButton m_addColor;
        private UICheckBox m_allowRed;
        private UICheckBox m_allowGreen;
        private UICheckBox m_allowBlues;
        private UICheckBox m_allowNeutral;



        public void Awake()
        {
            MainContainer = GetComponent<UIPanel>();
            MainContainer.autoLayout = true;
            MainContainer.clipChildren = true;
            MainContainer.autoLayoutDirection = LayoutDirection.Vertical;
            MainContainer.autoLayoutPadding = new RectOffset(0, 0, 4, 4);

            KlyteMonoUtils.CreateTabsComponent(out m_tabstrip, out UITabContainer m_tabContainer, MainContainer.transform, "TextEditor", new Vector4(0, 0, MainContainer.width, 40), new Vector4(0, 0, MainContainer.width, MainContainer.height - 40));
            UIPanel m_tabSettings = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Settings), "K45_ACE_BASICTAB_BASIC_SETTINGS", "RcSettings");
            UIPanel m_tabAppearence = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_AutoColorIcon), "K45_ACE_BASICTAB_APPEARANCE_SETTINGS", "RcAppearence");
            UIPanel m_tabLib = TabCommons.CreateNonScrollableTabLocalized(m_tabstrip, KlyteResourceLoader.GetDefaultSpriteNameFor(CommonsSpriteNames.K45_Load), "K45_ACE_BASICTAB_LIB_SETTINGS", "RcLib");

            var helperSettings = new UIHelperExtension(m_tabSettings, LayoutDirection.Vertical);
            var helperAppearence = new UIHelperExtension(m_tabAppearence, LayoutDirection.Vertical);
            var helperLib = new UIHelperExtension(m_tabLib, LayoutDirection.Vertical);


            AddTextField(Locale.Get("K45_ACE_BASICTAB_NAME"), out m_name, helperSettings, OnSetName);
            helperSettings.AddSpace(5);

            AddDropdown(Locale.Get("K45_ACE_BASICTAB_RULEFILTER"), out m_ruleFilter, helperSettings, Enum.GetNames(typeof(RuleCheckTypeVehicle)).Select(x => Locale.Get("K45_ACE_RULECHECKTYPE", x)).ToArray(), OnChangeRuleCheckType);
            AddButtonInEditorRow(m_ruleFilter, CommonsSpriteNames.K45_QuestionMark, Help_RuleFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_SERVICEFILTER"), out m_service, helperSettings, (Enum.GetValues(typeof(ItemClass.Service)) as ItemClass.Service[]).OrderBy(x => (int)x).Select(x => x == 0 ? Locale.Get("K45_ACE_ANYSERVICE_OPTION") : $"{x}").ToArray(), OnChangeServiceFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_SUBSERVICEFILTER"), out m_subService, helperSettings, Enum.GetNames(typeof(ItemClass.SubService)).Select(x => $"{x}").ToArray(), OnChangeSubServiceFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_LEVELFILTER"), out m_level, helperSettings, (Enum.GetValues(typeof(ItemClass.Level)) as ItemClass.Level[]).OrderBy(x => (int)x).Select(x => $"{x}").ToArray(), OnChangeLevelFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_CLASSFILTER"), out m_class, helperSettings, new string[0], OnChangeClassFilter);
            AddTextField(Locale.Get("K45_ACE_VEHICLERULES_ASSETSELECTSELF"), out m_assetFilterSelf, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilterSelf);
            m_popupSelf = ConfigureListSelectionPopupForUITextField(m_assetFilterSelf, () => AssetColorExpanderMod.Controller?.AssetsCache.FilterVehiclesByText(m_assetFilterSelf.text), OnAssetSelectedSelfChanged, GetCurrentSelectionNameSelf);
            m_popupSelf.height = 290;
            m_popupSelf.width -= 20;
            AddTextField(Locale.Get("K45_ACE_VEHICLERULES_ASSETSELECTOWNER"), out m_assetFilterOwner, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilterOwner);
            m_popupOwner = ConfigureListSelectionPopupForUITextField(m_assetFilterOwner, () => AssetColorExpanderMod.Controller?.AssetsCache.FilterBuildingsByText(m_assetFilterOwner.text), OnAssetSelectedOwnerChanged, GetCurrentSelectionNameOwner);
            m_popupOwner.height = 290;
            m_popupOwner.width -= 20;

            AddLibBox<ACEVehcileRuleLib, VehicleCityDataRuleXml>(helperLib, out m_copySettings, OnCopyRule, out m_pasteSettings, OnPasteRule, out _, null, OnLoadRule, GetRuleSerialized);

            AddCheckboxLocale("K45_ACE_VEHICLERULES_ALLOWWAGONWITHDIFFERENTCOLORS", out m_allowWagonDifferentColors, helperAppearence, OnAllowWagonDifferentColors);

            AddDropdown(Locale.Get("K45_ACE_COLORMODE"), out m_colorMode, helperAppearence, Enum.GetNames(typeof(ColoringMode)).Select(x => Locale.Get("K45_ACE_COLORINGMODE", x)).ToArray(), OnChangeColoringMode);
            AddButtonInEditorRow(m_colorMode, CommonsSpriteNames.K45_QuestionMark, Help_ColorMode);

            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWREDTONES", out m_allowRed, helperAppearence, OnAllowRedChanged);
            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWGREENTONES", out m_allowGreen, helperAppearence, OnAllowGreenChanged);
            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWBLUETONES", out m_allowBlues, helperAppearence, OnAllowBlueChanged);
            AddCheckboxLocale("K45_ACE_COLORMODE_ALLOWNEUTRALTONES", out m_allowNeutral, helperAppearence, OnAllowNeutralChanged);


            KlyteMonoUtils.CreateUIElement(out m_listColorContainer, helperAppearence.Self.transform, "listColors", new UnityEngine.Vector4(0, 0, helperAppearence.Self.width, helperAppearence.Self.height - 80));
            KlyteMonoUtils.CreateScrollPanel(m_listColorContainer, out m_colorListScroll, out _, m_listColorContainer.width - 20, m_listColorContainer.height);
            m_colorListScroll.backgroundSprite = "OptionsScrollbarTrack";
            m_colorListScroll.autoLayout = true;
            m_colorListScroll.autoLayoutDirection = LayoutDirection.Horizontal;
            m_colorListScroll.wrapLayout = true;
            CreateTemplateColorItem();
            m_colorFieldTemplateListColors = new UITemplateList<UIPanel>(m_colorListScroll, COLOR_SELECTOR_TEMPLATE);

            KlyteMonoUtils.InitCircledButton(m_colorListScroll, out m_addColor, CommonsSpriteNames.K45_Plus, (x, y) => AddColor(), "", 36);

            MainContainer.isVisible = false;
            m_pasteSettings.isVisible = false;
        }

        private void Help_ColorMode() => K45DialogControl.ShowModalHelp("Vehicle.ColoringMode", Locale.Get("K45_ACE_VEHICLERULES_COLORMODE"), 0);
        private void Help_RuleFilter() => K45DialogControl.ShowModalHelp("Vehicle.TypeOfRule", Locale.Get("K45_ACE_VEHICLERULES_RULEFILTER"), 0);
        private void AddColor() => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            x.m_colorList.Add(Color.white);
            UpdateColorList(ref x);
        });
        public void Start()
        {
            m_class.items = AssetColorExpanderMod.Controller?.ClassesCache.AllClassesVehicle?.Keys?.Select(x => x.name)?.OrderBy(x => x)?.ToArray() ?? new string[0];
            ACEPanel.Instance.VehicleTab.RuleList.EventSelectionChanged += OnChangeTab;
        }

        #region Prefab handling
        private string GetCurrentSelectionNameSelf()
        {
            string name = "";
            SafeObtain((ref VehicleCityDataRuleXml x) => name = x.AssetName);
            return name ?? "";
        }

        private string GetCurrentSelectionNameOwner()
        {
            string name = "";
            SafeObtain((ref VehicleCityDataRuleXml x) => name = x.BuildingName);
            return name ?? "";
        }
        #endregion

        #region ColorList

        private bool m_isLoadingColors = false;
        private void UpdateColorList(ref VehicleCityDataRuleXml reference)
        {
            UIPanel[] colorPickers = m_colorFieldTemplateListColors.SetItemCount(reference.m_colorList.Count);
            m_isLoadingColors = true;
            for (int i = 0; i < reference.m_colorList.Count; i++)
            {
                UIColorField colorField = colorPickers[i].GetComponentInChildren<UIColorField>();
                if (colorField.objectUserData == null)
                {
                    colorField.eventSelectedColorChanged += (x, y) =>
                        SafeObtain((ref VehicleCityDataRuleXml z) =>
                            {
                                if (!m_isLoadingColors && z.m_colorList.Count > x.parent.zOrder)
                                {
                                    m_isLoadingColors = true;
                                    if (y == default)
                                    {
                                        z.m_colorList.RemoveAt(x.parent.zOrder);
                                        UpdateColorList(ref z);
                                    }
                                    else
                                    {
                                        z.m_colorList[x.parent.zOrder] = y;
                                    }
                                    BuildingManager.instance.UpdateBuildingColors();
                                    m_isLoadingColors = false;
                                }
                            });
                    colorField.eventColorPickerOpen += KlyteMonoUtils.DefaultColorPickerHandler;
                    colorField.objectUserData = true;
                }
                colorField.selectedColor = reference.m_colorList[i];
            }
            m_addColor.zOrder = 99999999;
            m_isLoadingColors = false;
        }

        private void CreateTemplateColorItem()
        {
            if (!UITemplateUtils.GetTemplateDict().ContainsKey(COLOR_SELECTOR_TEMPLATE))
            {
                var go = new GameObject();
                UIPanel panel = go.AddComponent<UIPanel>();
                panel.size = new Vector2(36, 36);
                panel.autoLayout = true;
                panel.wrapLayout = false;
                panel.padding = new RectOffset(4, 4, 4, 4);
                panel.autoLayoutDirection = LayoutDirection.Horizontal;

                KlyteMonoUtils.CreateColorField(panel);

                UITemplateUtils.GetTemplateDict()[COLOR_SELECTOR_TEMPLATE] = panel;
            }
        }
        #endregion

        private delegate void SafeObtainMethod(ref VehicleCityDataRuleXml x);
        private void SafeObtain(SafeObtainMethod action, int? targetTab = null)
        {
            int effTargetTab = Math.Max(-1, targetTab ?? m_currentIdx);
            if (effTargetTab < 0)
            {
                return;
            }

            if (effTargetTab < ACEVehicleConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                action(ref ACEVehicleConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
                AssetColorExpanderMod.Controller.CleanCacheVehicle();
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
            SafeObtain((ref VehicleCityDataRuleXml x) =>
            {
                m_name.text = x.SaveName;

                m_ruleFilter.selectedIndex = (int)x.RuleCheckType;

                m_service.selectedIndex = (int)x.Service;
                m_subService.selectedIndex = (int)x.SubService;
                m_level.selectedIndex = (int)x.Level;
                m_class.selectedValue = x.ItemClassName;

                string targetAsset = x.AssetName ?? "";
                System.Collections.Generic.KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.AssetsCache.VehiclesLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterSelf.text = entry?.Key ?? "";


                targetAsset = x.BuildingName ?? "";
                entry = AssetColorExpanderMod.Controller?.AssetsCache.BuildingsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterOwner.text = entry?.Key ?? "";

                ApplyRuleCheck(x);

                m_allowWagonDifferentColors.isChecked = x.AllowDifferentColorsOnWagons;

                m_colorMode.selectedIndex = (int)x.ColoringMode;
                m_allowRed.isChecked = (x.PastelConfig & PastelConfig.AVOID_REDS) == 0;
                m_allowGreen.isChecked = (x.PastelConfig & PastelConfig.AVOID_GREENS) == 0;
                m_allowBlues.isChecked = (x.PastelConfig & PastelConfig.AVOID_BLUES) == 0;
                m_allowNeutral.isChecked = (x.PastelConfig & PastelConfig.AVOID_NEUTRALS) == 0;
                UpdateColorList(ref x);

                ApplyColorUIRules(x);

            });
        }

        private void ApplyColorUIRules(VehicleCityDataRuleXml x)
        {
            bool isPastel = x.ColoringMode == ColoringMode.PASTEL_FULL_VIVID || x.ColoringMode == ColoringMode.PASTEL_HIGHER_SATURATION || x.ColoringMode == ColoringMode.PASTEL_ORIG;
            m_allowRed.isVisible = isPastel;
            m_allowGreen.isVisible = isPastel;
            m_allowBlues.isVisible = isPastel;
            m_allowNeutral.isVisible = isPastel;
            m_listColorContainer.isVisible = x.ColoringMode == ColoringMode.LIST;
        }

        private void ApplyRuleCheck(VehicleCityDataRuleXml x)
        {
            m_service.parent.isVisible = x.RuleCheckType == RuleCheckTypeVehicle.SERVICE || x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_SUBSERVICE || x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_SUBSERVICE_LEVEL;
            m_subService.parent.isVisible = x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_SUBSERVICE || x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_SUBSERVICE_LEVEL;
            m_level.parent.isVisible = x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeVehicle.SERVICE_SUBSERVICE_LEVEL;
            m_class.parent.isVisible = x.RuleCheckType == RuleCheckTypeVehicle.ITEM_CLASS;
            m_assetFilterSelf.parent.isVisible = x.RuleCheckType == RuleCheckTypeVehicle.ASSET_NAME_SELF;
            m_assetFilterOwner.parent.isVisible = x.RuleCheckType == RuleCheckTypeVehicle.ASSET_NAME_OWNER;
        }

        private string m_clipboard;

        private string GetRuleSerialized()
        {
            int effTargetTab = Math.Max(-1, m_currentIdx);
            if (effTargetTab >= 0 && effTargetTab < ACEVehicleConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                return XmlUtils.DefaultXmlSerialize(ACEVehicleConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
            }
            else
            {
                return null;
            }
        }

        private void OnLoadRule(string obj) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            x = XmlUtils.DefaultXmlDeserialize<VehicleCityDataRuleXml>(obj);
            ACEPanel.Instance.VehicleTab.RuleList.FixTabstrip();
            ReloadData();
        });
        private void OnPasteRule() => OnLoadRule(m_clipboard);
        private void OnCopyRule() => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            m_clipboard = XmlUtils.DefaultXmlSerialize(x);
            m_pasteSettings.isVisible = true;
        });


        private void OnSetName(string text) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (!text.IsNullOrWhiteSpace())
            {
                x.SaveName = text;
                ACEPanel.Instance.VehicleTab.RuleList.FixTabstrip();
                OnChangeTab(m_currentIdx);
            }
            else
            {
                m_name.text = x.SaveName;
            }
        });

        private void OnChangeRuleCheckType(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            x.RuleCheckType = (RuleCheckTypeVehicle)sel;
            ApplyRuleCheck(x);
        });


        private void OnAssetSelectedSelfChanged(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0 && AssetColorExpanderMod.Controller.AssetsCache.VehiclesLoaded.TryGetValue(m_popupSelf.items[sel], out string assetName))
            {
                x.AssetName = assetName;
                m_assetFilterSelf.text = m_popupSelf.items[sel];
            }
            else
            {
                string targetAsset = x.AssetName ?? "";
                System.Collections.Generic.KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.AssetsCache.VehiclesLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterSelf.text = entry?.Key ?? "";
            }
        });
        private void OnAssetSelectedOwnerChanged(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0 && AssetColorExpanderMod.Controller.AssetsCache.BuildingsLoaded.TryGetValue(m_popupOwner.items[sel], out string assetName))
            {
                x.BuildingName = assetName;
                m_assetFilterOwner.text = m_popupOwner.items[sel];
            }
            else
            {
                string targetAsset = x.BuildingName ?? "";
                System.Collections.Generic.KeyValuePair<string, string>? entry = AssetColorExpanderMod.Controller?.AssetsCache.BuildingsLoaded.Where(y => y.Value == targetAsset).FirstOrDefault();
                m_assetFilterOwner.text = entry?.Key ?? "";
            }
        });
        private void OnAllowWagonDifferentColors(bool isChecked) => SafeObtain((ref VehicleCityDataRuleXml x) => x.AllowDifferentColorsOnWagons = isChecked);
        private void OnChangeClassFilter(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ItemClassName = m_class.items[sel];
            }
        });
        private void OnChangeLevelFilter(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Level = (ItemClass.Level)sel - 1;
            }
        });
        private void OnChangeSubServiceFilter(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.SubService = (ItemClass.SubService)sel;
            }
        });
        private void OnChangeServiceFilter(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Service = (ItemClass.Service)sel;
            }
        });
        private void OnChangeColoringMode(int sel) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ColoringMode = (ColoringMode)sel;
                ApplyColorUIRules(x);
            }
        });

        private void OnAllowRedChanged(bool isChecked) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_REDS;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_REDS;
            }
        });
        private void OnAllowGreenChanged(bool isChecked) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_GREENS;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_GREENS;
            }
        });
        private void OnAllowBlueChanged(bool isChecked) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_BLUES;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_BLUES;
            }
        });
        private void OnAllowNeutralChanged(bool isChecked) => SafeObtain((ref VehicleCityDataRuleXml x) =>
        {
            if (isChecked)
            {
                x.PastelConfig &= ~PastelConfig.AVOID_NEUTRALS;
            }
            else
            {
                x.PastelConfig |= PastelConfig.AVOID_NEUTRALS;
            }
        });
    }

}
