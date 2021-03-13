using ColossalFramework.Globalization;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.Libraries;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;

namespace Klyte.AssetColorExpander.UI
{
    public class ACEPropRulesList : BasicRulesList<PropCityDataRuleXml, ACEPropRulesetLib, ACERulesetContainer<PropCityDataRuleXml>>
    {
        protected override ref PropCityDataRuleXml[] ReferenceData => ref ACEPropConfigRulesData.Instance.Rules.m_dataArray;

        protected override string LocaleRuleListTitle => "K45_ACE_PROPRULES_RULELISTTITLE";

        protected override string LocaleImport => "K45_ACE_PROPRULES_IMPORTRULELIST";

        protected override string LocaleExport => "K45_ACE_PROPRULES_EXPORTRULELIST";

        protected override void Help_RulesList() => K45DialogControl.ShowModalHelp("General.RuleList", Locale.Get("K45_ACE_PROPRULES_RULELISTTITLE"),0, ACEPropRulesetLib.Instance.DefaultXmlFileBaseFullPath);
        protected override void OnTabstripFix() => AssetColorExpanderMod.Controller?.CleanCacheProp();
    }
}