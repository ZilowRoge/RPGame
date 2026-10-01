using System;
using System.Collections.Generic;
using RPGame.Enemies;
using UnityEngine;

namespace RPGame.Encounter
{
    public sealed class EnemyPool : MonoBehaviour
    {
        private readonly Dictionary<EnemyDefinition, Queue<PooledEnemy>> availableByDefinition = new();
        private readonly Dictionary<PooledEnemy, EnemyDefinition> definitionByInstance = new();
        private readonly HashSet<PooledEnemy> activeInstances = new();

        public void Prewarm(IEnumerable<EnemyDefinition> enemyDefinitions)
        {
            if (enemyDefinitions == null)
            {
                return;
            }

            foreach (EnemyDefinition definition in enemyDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                ValidateDefinition(definition);
                Queue<PooledEnemy> available = GetOrCreateAvailableQueue(definition);
                for (int i = 0; i < definition.PrewarmCount; i++)
                {
                    PooledEnemy instance = CreateInstance(definition);
                    instance.gameObject.SetActive(false);
                    available.Enqueue(instance);
                }
            }
        }

        public PooledEnemy Acquire(EnemyDefinition definition, Vector3 position, Quaternion rotation)
        {
            ValidateDefinition(definition);

            Queue<PooledEnemy> available = GetOrCreateAvailableQueue(definition);
            PooledEnemy instance = available.Count > 0
                ? available.Dequeue()
                : CreateInstance(definition);

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            instance.OnSpawned();
            activeInstances.Add(instance);
            return instance;
        }

        public void Release(PooledEnemy instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!definitionByInstance.TryGetValue(instance, out EnemyDefinition definition))
            {
                throw new InvalidOperationException("Cannot release an enemy that does not belong to this pool.");
            }

            if (!activeInstances.Remove(instance))
            {
                throw new InvalidOperationException("Cannot release an enemy that is not currently acquired.");
            }

            instance.OnDespawned();
            instance.gameObject.SetActive(false);
            GetOrCreateAvailableQueue(definition).Enqueue(instance);
        }

        private PooledEnemy CreateInstance(EnemyDefinition definition)
        {
            GameObject instanceObject = Instantiate(definition.Prefab, transform);
            PooledEnemy instance = instanceObject.GetComponent<PooledEnemy>();
            definitionByInstance.Add(instance, definition);
            return instance;
        }

        private Queue<PooledEnemy> GetOrCreateAvailableQueue(EnemyDefinition definition)
        {
            if (!availableByDefinition.TryGetValue(definition, out Queue<PooledEnemy> available))
            {
                available = new Queue<PooledEnemy>();
                availableByDefinition.Add(definition, available);
            }

            return available;
        }

        private static void ValidateDefinition(EnemyDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.Prefab == null)
            {
                throw new InvalidOperationException("Enemy definition must have a prefab before it can be pooled.");
            }

            if (definition.Prefab.GetComponent<PooledEnemy>() == null)
            {
                throw new InvalidOperationException("Enemy prefab root must contain a PooledEnemy component.");
            }
        }
    }
}
