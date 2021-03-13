using Klyte.Commons.Extensors;

namespace Klyte.AssetColorExpander
{
    public class ACENetsOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(NetManager).GetMethod("ReleaseSegment"), null, typeof(ACENetsOverrides).GetMethod("AfterReleaseSegment", RedirectorUtils.allFlags));


        public static void AfterReleaseSegment(ushort segment)
        {
            if (AssetColorExpanderMod.Controller != null && AssetColorExpanderMod.Controller.UpdatedRulesSubPropsNets != null)
            {
                AssetColorExpanderMod.Controller.UpdatedRulesSubPropsNets[segment] = null;
            }
        }
    }
}