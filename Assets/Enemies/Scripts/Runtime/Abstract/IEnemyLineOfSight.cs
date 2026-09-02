using UnityEngine;

namespace RPGame.Enemies
{
    public interface IEnemyLineOfSight
    {
        bool HasLineOfSight(Vector3 targetPosition);
        bool HasLineOfSightFrom(Vector3 origin, Vector3 targetPosition);
    }
}
