using UnityEngine;

namespace RPGame.Core.Movement
{
    public interface IKnockbackable
    {
        void ApplyKnockback(Vector3 direction, float distance, float duration);
    }
}
