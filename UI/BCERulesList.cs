using Klyte.BuildingColorExpander.Data;
using Klyte.BuildingColorExpander.Libraries;
using Klyte.BuildingColorExpander.XML;
using Klyte.Commons.UI;

namespace Klyte.BuildingColorExpander.UI
{
    public class BCERulesList : BasicRulesList<CityDataRuleXml, BCERulesetConfigLib, BCEConfig<CityDataRuleXml>>
    {
        protected override ref CityDataRuleXml[] ReferenceData => ref BCEConfigRulesData.Instance.Rules.m_dataArray;

        protected override string LocaleRuleListTitle => "K45_BCE_BUILDINGRULES_RULELISTTITLE";

        protected override string LocaleImport => "K45_BCE_BUILDINGRULES_IMPORTRULELIST";

        protected override string LocaleExport => "K45_BCE_BUILDINGRULES_EXPORTRULELIST";

        protected override void Help_RulesList() { }
        protected override void OnTabstripFix() => BuildingColorExpanderMod.Controller?.CleanCache();
    }
}