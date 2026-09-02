using UnityEngine;

namespace RPGame.Enemies
{
    public interface IEnemyGroundProjection
    {
        bool TryProjectToGround(Vector3 candidatePosition, out Vector3 groundPosition);
    }
}
