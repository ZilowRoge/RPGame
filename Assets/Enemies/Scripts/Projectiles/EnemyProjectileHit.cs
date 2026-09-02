using UnityEngine;

namespace RPGame.Enemies
{
    public readonly struct EnemyProjectileHit
    {
        public EnemyProjectileHit(Collider collider, Vector3 point, Vector3 normal)
        {
            Collider = collider;
            Point = point;
            Normal = normal;
        }

        public Collider Collider { get; }
        public Vector3 Point { get; }
        public Vector3 Normal { get; }
    }
}
