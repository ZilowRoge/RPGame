using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Encounter
{
    public sealed class SpawnPoint : MonoBehaviour
    {
        private Coroutine spawnCoroutine;
        private SpawnManager activeManager;
        private int activeOperationId;

        internal bool IsSpawning => spawnCoroutine != null;

        internal void StartSpawnQueue(
            IReadOnlyList<EnemyDefinition> enemies,
            float spawnDelay,
            EnemyPool enemyPool,
            SpawnManager manager,
            int operationId)
        {
            CancelSpawning();

            activeManager = manager;
            activeOperationId = operationId;
            spawnCoroutine = StartCoroutine(SpawnQueue(enemies, spawnDelay, enemyPool));
        }

        internal void CancelSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            activeManager = null;
            activeOperationId = 0;
        }

        private IEnumerator SpawnQueue(IReadOnlyList<EnemyDefinition> enemies, float spawnDelay, EnemyPool enemyPool)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Spawn(enemies[i], enemyPool);

                if (i < enemies.Count - 1 && spawnDelay > 0f)
                {
                    yield return new WaitForSeconds(spawnDelay);
                }
            }

            spawnCoroutine = null;
            SpawnManager manager = activeManager;
            int operationId = activeOperationId;
            activeManager = null;
            activeOperationId = 0;
            manager?.NotifySpawnPointFinished(this, operationId);
        }

        private void Spawn(EnemyDefinition enemyDefinition, EnemyPool enemyPool)
        {
            if (enemyDefinition == null || enemyPool == null)
            {
                return;
            }

            enemyPool.Acquire(enemyDefinition, transform.position, transform.rotation);
        }
    }
}
