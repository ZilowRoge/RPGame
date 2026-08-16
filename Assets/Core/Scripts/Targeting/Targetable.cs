using UnityEngine;

namespace RPGame.Core.Targeting
{
    [ExecuteAlways]
    public sealed class Targetable : MonoBehaviour, ITargetable
    {
        [SerializeField] private Transform targetPoint;

        public Transform TargetPoint => targetPoint != null ? targetPoint : transform;

        private void OnEnable()
        {
            TargetRegistry.Register(this);
        }

        private void OnDisable()
        {
            TargetRegistry.Unregister(this);
        }

        private void OnDestroy()
        {
            TargetRegistry.Unregister(this);
        }
    }
}
