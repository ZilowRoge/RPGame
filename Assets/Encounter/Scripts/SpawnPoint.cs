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
            SpawnManager manager,
            int operationId)
        {
            CancelSpawning();

            activeManager = manager;
            activeOperationId = operationId;
            spawnCoroutine = StartCoroutine(SpawnQueue(enemies, spawnDelay));
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

        private IEnumerator SpawnQueue(IReadOnlyList<EnemyDefinition> enemies, float spawnDelay)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Spawn(enemies[i]);

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

        private void Spawn(EnemyDefinition enemyDefinition)
        {
            if (enemyDefinition == null || enemyDefinition.Prefab == null)
            {
                return;
            }

            Instantiate(enemyDefinition.Prefab, transform.position, transform.rotation);
        }
    }
}
