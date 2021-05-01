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
    public class ACEVehicleRulesList : BasicRulesList<VehicleCityDataRuleXml, ACEVehicleRulesetLib, ACERulesetContainer<VehicleCityDataRuleXml>>
    {

        protected IEnumerator CleanCacheNextFrame()
        {
            yield return new WaitForEndOfFrame();
            AssetColorExpanderMod.Controller.CleanCacheVehicle();
        }
        protected override ref VehicleCityDataRuleXml[] ReferenceData => ref ACEVehicleConfigRulesData.Instance.Rules.m_dataArray;

        protected override string LocaleRuleListTitle => "K45_ACE_VEHICLERULES_RULELISTTITLE";

        protected override string LocaleImport => "K45_ACE_VEHICLERULES_IMPORTRULELIST";

        protected override string LocaleExport => "K45_ACE_VEHICLERULES_EXPORTRULELIST";

        protected override void Help_RulesList() => K45DialogControl.ShowModalHelp("General.RuleList", Locale.Get("K45_ACE_VEHICLERULES_RULELISTTITLE"),0, ACEBuildingRulesetLib.Instance.DefaultXmlFileBaseFullPath);
        protected override void OnTabstripFix() => StartCoroutine(CleanCacheNextFrame());
    }
}