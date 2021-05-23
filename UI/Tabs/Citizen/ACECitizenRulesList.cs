using ColossalFramework.Globalization;
using Klyte.AssetColorExpander.Data;
using Klyte.AssetColorExpander.Libraries;
using Klyte.AssetColorExpander.XML;
using Klyte.Commons.UI;
using Klyte.Commons.Utils;
using System.Collections;
using UnityEngine;

namespace Klyte.AssetColorExpander.UI
{
    public class ACECitizenRulesList : BasicRulesList<CitizenCityDataRuleXml, ACECitizenRulesetLib, ACERulesetContainer<CitizenCityDataRuleXml>>
    {

        protected IEnumerator CleanCacheNextFrame()
        {
            yield return new WaitForEndOfFrame();
            AssetColorExpanderMod.Controller.CleanCacheCitizen();
        }
        protected override ref CitizenCityDataRuleXml[] ReferenceData => ref ACECitizenConfigRulesData.Instance.Rules.m_dataArray;

        protected override string LocaleRuleListTitle => "K45_ACE_CITIZENRULES_RULELISTTITLE";

        protected override string LocaleImport => "K45_ACE_CITIZENRULES_IMPORTRULELIST";

        protected override string LocaleExport => "K45_ACE_CITIZENRULES_EXPORTRULELIST";

        protected override void Help_RulesList() => K45DialogControl.ShowModalHelp("General.RuleList", Locale.Get("K45_ACE_CITIZENRULES_RULELISTTITLE"),0, ACEBuildingRulesetLib.Instance.DefaultXmlFileBaseFullPath);
        protected override void OnTabstripFix() => StartCoroutine(CleanCacheNextFrame());
    }
}