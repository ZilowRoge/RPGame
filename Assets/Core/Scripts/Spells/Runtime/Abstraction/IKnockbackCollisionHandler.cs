using UnityEngine;

namespace RPGame.Core.Spells
{
    public interface IKnockbackCollisionHandler : IRuntimeSpellBehavior
    {
        void OnKnockbackCollision(GameObject target, Collider obstacle, Vector3 point);
    }
}
