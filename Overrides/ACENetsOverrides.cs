using Klyte.Commons.Extensions;

namespace Klyte.AssetColorExpander
{
    public class ACENetsOverrides : Redirector, IRedirectable
    {
        public void Awake() => AddRedirect(typeof(NetManager).GetMethod("ReleaseSegment"), null, typeof(ACENetsOverrides).GetMethod("AfterReleaseSegment", RedirectorUtils.allFlags));


        public static void AfterReleaseSegment(ushort segment) => AssetColorExpanderMod.Controller.CachedColorSubPropsNets[segment] = null;
    }
}