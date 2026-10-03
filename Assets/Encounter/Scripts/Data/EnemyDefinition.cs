using UnityEngine;

namespace RPGame.Encounter
{
    [CreateAssetMenu(fileName = "EnemyDefinition", menuName = "RPGame/Encounter/Enemy Definition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int cost = 1;
        [SerializeField] private int unlockWave = 1;
        [SerializeField] private int prewarmCount;

        public GameObject Prefab => prefab;
        public int Cost => cost;
        public int UnlockWave => unlockWave;
        public int PrewarmCount => prewarmCount;

        private void OnValidate()
        {
            cost = Mathf.Max(1, cost);
            unlockWave = Mathf.Max(1, unlockWave);
            prewarmCount = Mathf.Max(0, prewarmCount);
        }
    }
}
