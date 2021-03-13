using ColossalFramework.Globalization;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.Libraries;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;

namespace Klyte.AssetColorExpander.UI
{
    public class ACEBuildingRulesList : BasicRulesList<BuildingCityDataRuleXml, ACEBuildingRulesetLib, ACERulesetContainer<BuildingCityDataRuleXml>>
    {
        protected override ref BuildingCityDataRuleXml[] ReferenceData => ref ACEBuildingConfigRulesData.Instance.Rules.m_dataArray;

        protected override string LocaleRuleListTitle => "K45_ACE_BUILDINGRULES_RULELISTTITLE";

        protected override string LocaleImport => "K45_ACE_BUILDINGRULES_IMPORTRULELIST";

        protected override string LocaleExport => "K45_ACE_BUILDINGRULES_EXPORTRULELIST";

        protected override void Help_RulesList() => K45DialogControl.ShowModalHelp("General.RuleList", Locale.Get("K45_ACE_BUILDINGRULES_RULELISTTITLE"),0, ACEBuildingRulesetLib.Instance.DefaultXmlFileBaseFullPath);
        protected override void OnTabstripFix() => AssetColorExpanderMod.Controller?.CleanCacheBuilding();
    }
}