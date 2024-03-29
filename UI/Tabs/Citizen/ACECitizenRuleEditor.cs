﻿using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.Libraries;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.Extensions;
using Klyte.Commons.UI.SpriteNames;
using Klyte.Commons.Utils;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using static Klyte.AssetColorExpander.UI.ACECommonsUI;
using static Klyte.Commons.UI.DefaultEditorUILib;

namespace Klyte.AssetColorExpander.UI
{
    public class ACECitizenRuleEditor : UICustomControl
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
        private UIDropDown m_ai;
        private UITextField m_assetFilterSelf;
        private UIListBox m_popupSelf;
        private UIPanel m_exportButtonContainer;
        private UIButton m_exportButton;
        private UIPanel m_exportButtonContainerLocal;
        private UIButton m_exportButtonLocal;

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

            AddDropdown(Locale.Get("K45_ACE_BASICTAB_RULEFILTER"), out m_ruleFilter, helperSettings, Enum.GetNames(typeof(RuleCheckTypeCitizen)).Select(x => Locale.Get("K45_ACE_RULECHECKTYPE", x)).ToArray(), OnChangeRuleCheckType);
            AddButtonInEditorRow(m_ruleFilter, CommonsSpriteNames.K45_QuestionMark, Help_RuleFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_SERVICEFILTER"), out m_service, helperSettings, (Enum.GetValues(typeof(ItemClass.Service)) as ItemClass.Service[]).OrderBy(x => (int)x).Select(x => x == 0 ? Locale.Get("K45_ACE_ANYSERVICE_OPTION") : $"{x}").ToArray(), OnChangeServiceFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_SUBSERVICEFILTER"), out m_subService, helperSettings, Enum.GetNames(typeof(ItemClass.SubService)).Select(x => $"{x}").ToArray(), OnChangeSubServiceFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_LEVELFILTER"), out m_level, helperSettings, (Enum.GetValues(typeof(ItemClass.Level)) as ItemClass.Level[]).OrderBy(x => (int)x).Select(x => $"{x}").ToArray(), OnChangeLevelFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_CLASSFILTER"), out m_class, helperSettings, new string[0], OnChangeClassFilter);
            AddDropdown(Locale.Get("K45_ACE_BASICTAB_AIFILTER"), out m_ai, helperSettings, new string[0], OnChangeAIFilter);
            AddTextField(Locale.Get("K45_ACE_CITIZENRULES_ASSETSELECT"), out m_assetFilterSelf, helperSettings, null);

            KlyteMonoUtils.UiTextFieldDefaultsForm(m_assetFilterSelf);
            m_popupSelf = ConfigureListSelectionPopupForUITextField(m_assetFilterSelf, (text) => AssetColorExpanderMod.Controller?.AssetsCache.FilterCitizensByText(text), OnAssetSelectedSelfChanged);
            m_popupSelf.height = 290;
            m_popupSelf.width -= 20;

            GenerateExportButtons(helperSettings, "Citizen",
            out m_exportButtonContainer, out m_exportButton, OnExport,
            out m_exportButtonContainerLocal, out m_exportButtonLocal, OnExportLocal);

            AddLibBox<ACECitizenRuleLib, CitizenCityDataRuleXml>(helperLib, out m_copySettings, OnCopyRule, out m_pasteSettings, OnPasteRule, out _, null, OnLoadRule, GetRuleSerialized);


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


        private void Help_ColorMode() => K45DialogControl.ShowModalHelp("Citizen.ColoringMode", Locale.Get("K45_ACE_CITIZENRULES_COLORMODE"), 0);
        private void Help_RuleFilter() => K45DialogControl.ShowModalHelp("Citizen.TypeOfRule", Locale.Get("K45_ACE_CITIZENRULES_RULEFILTER"), 0);
        private void AddColor() => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            x.m_colorList.Add(Color.white);
            UpdateColorList(ref x);
        });
        public void Start()
        {
            m_class.items = AssetColorExpanderMod.Controller?.ClassesCache.AllClassesCitizen?.Keys?.Select(x => x.name)?.OrderBy(x => x)?.ToArray() ?? new string[0];
            m_ai.items = AssetColorExpanderMod.Controller?.ClassesCache.AllAICitizen?.Keys?.Select(x => x.Name)?.OrderBy(x => x)?.ToArray() ?? new string[0];
            ACEPanel.Instance.CitizenTab.RuleList.EventSelectionChanged += OnChangeTab;
        }

        #region Prefab handling
        private string GetCurrentSelectionNameSelf()
        {
            string name = "";
            SafeObtain((ref CitizenCityDataRuleXml x) => name = x.AssetName);
            return name ?? "";
        }

        #endregion

        #region ColorList

        private bool m_isLoadingColors = false;
        private void UpdateColorList(ref CitizenCityDataRuleXml reference)
        {
            UIPanel[] colorPickers = m_colorFieldTemplateListColors.SetItemCount(reference.m_colorList.Count);
            m_isLoadingColors = true;
            for (int i = 0; i < reference.m_colorList.Count; i++)
            {
                UIColorField colorField = colorPickers[i].GetComponentInChildren<UIColorField>();
                if (colorField.objectUserData == null)
                {
                    colorField.eventSelectedColorChanged += (x, y) =>
                        SafeObtain((ref CitizenCityDataRuleXml z) =>
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

        private delegate void SafeObtainMethod(ref CitizenCityDataRuleXml x);
        private void SafeObtain(SafeObtainMethod action, int? targetTab = null)
        {
            int effTargetTab = Math.Max(-1, targetTab ?? m_currentIdx);
            if (effTargetTab < 0)
            {
                return;
            }

            if (effTargetTab < ACECitizenConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                action(ref ACECitizenConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
                AssetColorExpanderMod.Controller.CleanCacheCitizen();
            }
        }
        private void OnChangeTab(int obj)
        {
            MainContainer.isVisible = obj >= 0;
            m_currentIdx = obj;
            ReloadData();
        }

        private void ReloadData() => SafeObtain((ref CitizenCityDataRuleXml x) =>
                                   {
                                       m_name.text = x.SaveName ?? "";

                                       m_ruleFilter.selectedIndex = (int)x.RuleCheckType;

                                       m_service.selectedIndex = (int)x.Service;
                                       m_subService.selectedIndex = (int)x.SubService;
                                       m_level.selectedIndex = (int)x.Level+1;
                                       m_class.selectedValue = x.ItemClassName;
                                       m_ai.selectedValue = x.AiClassName;

                                       string targetAsset = x.AssetName ?? "";
                                       m_assetFilterSelf.text = CitizenIndexes.instance.PrefabsLoaded.Where(y => y.Value.name == targetAsset).FirstOrDefault().Key ?? "";

                                       ApplyRuleCheck(x);


                                       m_colorMode.selectedIndex = (int)x.ColoringMode;
                                       m_allowRed.isChecked = (x.PastelConfig & PastelConfig.AVOID_REDS) == 0;
                                       m_allowGreen.isChecked = (x.PastelConfig & PastelConfig.AVOID_GREENS) == 0;
                                       m_allowBlues.isChecked = (x.PastelConfig & PastelConfig.AVOID_BLUES) == 0;
                                       m_allowNeutral.isChecked = (x.PastelConfig & PastelConfig.AVOID_NEUTRALS) == 0;
                                       UpdateColorList(ref x);

                                       ApplyColorUIRules(x);

                                   });

        private void ApplyColorUIRules(CitizenCityDataRuleXml x)
        {
            bool isPastel = x.ColoringMode == ColoringMode.PASTEL_FULL_VIVID || x.ColoringMode == ColoringMode.PASTEL_HIGHER_SATURATION || x.ColoringMode == ColoringMode.PASTEL_ORIG;
            m_allowRed.isVisible = isPastel;
            m_allowGreen.isVisible = isPastel;
            m_allowBlues.isVisible = isPastel;
            m_allowNeutral.isVisible = isPastel;
            m_listColorContainer.isVisible = x.ColoringMode == ColoringMode.LIST;
        }

        private void ApplyRuleCheck(CitizenCityDataRuleXml x)
        {
            m_service.parent.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.SERVICE || x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_SUBSERVICE || x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_SUBSERVICE_LEVEL;
            m_subService.parent.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_SUBSERVICE || x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_SUBSERVICE_LEVEL;
            m_level.parent.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_LEVEL || x.RuleCheckType == RuleCheckTypeCitizen.SERVICE_SUBSERVICE_LEVEL;
            m_class.parent.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.ITEM_CLASS;
            m_assetFilterSelf.parent.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.ASSET_NAME;
            m_ai.parent.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.AI;

            m_exportButtonContainer.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.ASSET_NAME;
            m_exportButtonContainerLocal.isVisible = x.RuleCheckType == RuleCheckTypeCitizen.ASSET_NAME;

            if (ulong.TryParse(x.AssetName?.Split('.')[0], out _))
            {
                m_exportButton.Enable();
            }
            else
            {
                m_exportButton.Disable();
            }
            if (x.AssetName.IsNullOrWhiteSpace())
            {
                m_exportButtonLocal.Disable();
            }
            else
            {
                m_exportButtonLocal.Enable();
            }
        }

        private string m_clipboard;

        private string GetRuleSerialized()
        {
            int effTargetTab = Math.Max(-1, m_currentIdx);
            if (effTargetTab >= 0 && effTargetTab < ACECitizenConfigRulesData.Instance.Rules.m_dataArray.Length)
            {
                return XmlUtils.DefaultXmlSerialize(ACECitizenConfigRulesData.Instance.Rules.m_dataArray[effTargetTab]);
            }
            else
            {
                return null;
            }
        }

        private void OnLoadRule(string obj) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            x = XmlUtils.DefaultXmlDeserialize<CitizenCityDataRuleXml>(obj);
            ACEPanel.Instance.CitizenTab.RuleList.FixTabstrip();
            ReloadData();
        });
        private void OnPasteRule() => OnLoadRule(m_clipboard);
        private void OnCopyRule() => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            m_clipboard = XmlUtils.DefaultXmlSerialize(x);
            m_pasteSettings.isVisible = true;
        });


        private void OnSetName(string text) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (!text.IsNullOrWhiteSpace())
            {
                x.SaveName = text;
                ACEPanel.Instance.CitizenTab.RuleList.FixTabstrip();
                OnChangeTab(m_currentIdx);
            }
            else
            {
                m_name.text = x.SaveName ?? "";
            }
        });

        private void OnChangeRuleCheckType(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            x.RuleCheckType = (RuleCheckTypeCitizen)sel;
            ApplyRuleCheck(x);
        });


        private string OnAssetSelectedSelfChanged(string input, int arg1, string[] arg2) =>
            ACEAssetCache.GetFromInfoIndex<CitizenIndexes, CitizenInfo>(input, arg1, arg2, m_assetFilterSelf, (result, info) =>
             {
                 SafeObtain((ref CitizenCityDataRuleXml x) =>
                 {
                     x.AssetName = info?.name ?? "";
                     ApplyRuleCheck(x);
                 });
             });

        private void OnChangeClassFilter(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ItemClassName = m_class.items[sel];
            }
        });

        private void OnChangeAIFilter(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.AiClassName = m_ai.items[sel];
            }
        });
        private void OnChangeLevelFilter(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Level = (ItemClass.Level)sel - 1;
            }
        });
        private void OnChangeSubServiceFilter(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.SubService = (ItemClass.SubService)sel;
            }
        });
        private void OnChangeServiceFilter(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.Service = (ItemClass.Service)sel;
            }
        });
        private void OnChangeColoringMode(int sel) => SafeObtain((ref CitizenCityDataRuleXml x) =>
        {
            if (sel >= 0)
            {
                x.ColoringMode = (ColoringMode)sel;
                ApplyColorUIRules(x);
            }
        });

        private void OnAllowRedChanged(bool isChecked) => SafeObtain((ref CitizenCityDataRuleXml x) =>
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
        private void OnAllowGreenChanged(bool isChecked) => SafeObtain((ref CitizenCityDataRuleXml x) =>
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
        private void OnAllowBlueChanged(bool isChecked) => SafeObtain((ref CitizenCityDataRuleXml x) =>
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
        private void OnAllowNeutralChanged(bool isChecked) => SafeObtain((ref CitizenCityDataRuleXml x) =>
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

        private void OnExport() => SafeObtain((ref CitizenCityDataRuleXml x) => FileUtils.DoInPrefabFolder(x.AssetName,
                                     (folder) =>
                                     {
                                         string currentDataSerial = GetRuleSerialized();
                                         CitizenAssetFolderRuleXml asAssetRule = XmlUtils.DefaultXmlDeserialize<CitizenAssetFolderRuleXml>(currentDataSerial);
                                         var container = new ACERulesetContainer<CitizenAssetFolderRuleXml>
                                         {
                                             m_dataArray = new CitizenAssetFolderRuleXml[]
                                             {
                           asAssetRule
                                             }
                                         };
                                         string targetData = XmlUtils.DefaultXmlSerialize(container);
                                         File.WriteAllText(Path.Combine(folder, ACELoadedDataContainer.DEFAULT_XML_NAME_CITIZEN), targetData);
                                     })
            );
        private void OnExportLocal() => SafeObtain((ref CitizenCityDataRuleXml x) =>
                                      {
                                          FileUtils.EnsureFolderCreation(ACEController.FOLDER_PATH_GENERAL_CONFIG);
                                          string filename = Path.Combine(ACEController.FOLDER_PATH_GENERAL_CONFIG, ACELoadedDataContainer.DEFAULT_XML_NAME_CITIZEN);
                                          string currentDataSerial = GetRuleSerialized();
                                          CitizenAssetFolderRuleXml asAssetRule = XmlUtils.DefaultXmlDeserialize<CitizenAssetFolderRuleXml>(currentDataSerial);
                                          ACERulesetContainer<CitizenAssetFolderRuleXml> container = File.Exists(filename) ? XmlUtils.DefaultXmlDeserialize<ACERulesetContainer<CitizenAssetFolderRuleXml>>(File.ReadAllText(filename)) : new ACERulesetContainer<CitizenAssetFolderRuleXml>();
                                          container.m_dataArray = container.m_dataArray.Where(y => y.AssetName != asAssetRule.AssetName).Union(new CitizenAssetFolderRuleXml[] { asAssetRule }).ToArray();
                                          File.WriteAllText(filename, XmlUtils.DefaultXmlSerialize(container));
                                      }
            );
    }

}
