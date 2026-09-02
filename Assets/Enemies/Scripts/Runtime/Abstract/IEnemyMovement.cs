using UnityEngine;

namespace RPGame.Enemies
{
    public interface IEnemyMovement
    {
        Vector3 Position { get; }

        void MoveTo(Vector3 position);
        void Stop();
        bool TryResolvePosition(Vector3 desiredPosition, out Vector3 validPosition);
    }
}
