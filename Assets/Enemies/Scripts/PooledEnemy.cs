using System;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class PooledEnemy : MonoBehaviour
    {
        public bool IsSpawned { get; private set; }

        public void OnSpawned()
        {
            if (IsSpawned)
            {
                throw new InvalidOperationException("Enemy is already spawned.");
            }

            IsSpawned = true;
        }

        public void OnDespawned()
        {
            if (!IsSpawned)
            {
                throw new InvalidOperationException("Enemy is not spawned.");
            }

            IsSpawned = false;
        }
    }
}
