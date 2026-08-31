using UnityEngine;

namespace RPGame.Enemies
{
    public interface IEnemyLineOfSight
    {
        bool HasLineOfSight(Vector3 targetPosition);
    }
}
