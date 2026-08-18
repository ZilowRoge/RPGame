using UnityEngine;

namespace RPGame.Core.Targeting
{
    public abstract class Targetable : MonoBehaviour, ITargetable
    {
        [SerializeField] private Transform targetPoint;

        public Transform TargetPoint => targetPoint != null ? targetPoint : transform;
    }
}
