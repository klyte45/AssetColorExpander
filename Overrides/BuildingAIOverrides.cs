using Klyte.BuildingColorExpander.Extensors;
using Klyte.Commons.Extensors;
using UnityEngine;

namespace Klyte.BuildingColorExpander.Overrides
{

    internal class BuildingAIOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; internal set; }

        public void Awake()
        {
            RedirectorInstance = new Redirector();
            RedirectorInstance.AddRedirect(typeof(BuildingAI).GetMethod("GetColor"), typeof(BCEColoringConfiguration).GetMethod("PreGetColor", RedirectorUtils.allFlags));
            BCEColoringConfiguration.ReloadFiles();
        }
    }
}
