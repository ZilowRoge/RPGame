using UnityEngine;

namespace RPGame.Core.Targeting
{
    [ExecuteAlways]
    public sealed class PlayerTargetable : Targetable
    {
        private void OnEnable()
        {
            TargetRegistry.RegisterTarget(this);
        }

        private void OnDisable()
        {
            TargetRegistry.UnregisterTarget(this);
        }

        private void OnDestroy()
        {
            TargetRegistry.UnregisterTarget(this);
        }
    }
}
