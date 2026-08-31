using UnityEngine;

namespace RPGame.Enemies
{
    public interface IEnemyMovement
    {
        void MoveTo(Vector3 position);
        void Stop();
    }
}
