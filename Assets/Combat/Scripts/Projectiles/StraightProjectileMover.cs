using UnityEngine;

namespace RPGame.Combat.Projectiles
{
    public sealed class StraightProjectileMover : ProjectileMover
    {
        public override void Tick(float deltaTime)
        {
            transform.position += transform.forward * Projectile.CurrentSpeed * deltaTime;
        }
    }
}
