using System;
using System.Collections.Generic;
using RPGame.Core.Pooling;
using UnityEngine;

namespace RPGame.Enemies
{
    public sealed class PooledEnemy : MonoBehaviour
    {
        private readonly List<IPooledEnemyResettable> resettableComponents = new();

        public bool IsSpawned { get; private set; }

        private void Awake()
        {
            CacheResettableComponents();
        }

        public void OnSpawned()
        {
            if (IsSpawned)
            {
                throw new InvalidOperationException("Enemy is already spawned.");
            }

            for (int i = 0; i < resettableComponents.Count; i++)
            {
                resettableComponents[i].ResetForSpawn();
            }

            IsSpawned = true;
        }

        public void OnDespawned()
        {
            if (!IsSpawned)
            {
                throw new InvalidOperationException("Enemy is not spawned.");
            }

            for (int i = 0; i < resettableComponents.Count; i++)
            {
                resettableComponents[i].ResetForDespawn();
            }

            IsSpawned = false;
        }

        private void CacheResettableComponents()
        {
            resettableComponents.Clear();
            MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPooledEnemyResettable resettable)
                {
                    resettableComponents.Add(resettable);
                }
            }
        }
    }
}
