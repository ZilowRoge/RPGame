using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Projectiles
{
    public abstract class ProjectileMover : MonoBehaviour
    {
        protected ProjectileController Projectile { get; private set; }
        protected IProjectileMovementSource MovementSource { get; private set; }
        protected CasterData CasterData { get; private set; }

        public virtual void Initialize(IProjectileMovementSource movementSource)
        {
            MovementSource = movementSource;
        }

        public virtual void Initialize(ProjectileController projectile, CasterData casterData)
        {
            Projectile = projectile;
            MovementSource = projectile;
            CasterData = casterData;
        }

        public abstract void Tick(float deltaTime);
    }
}
