using UnityEngine;

namespace RPGame.Encounter
{
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "RPGame/Encounter/Score Config")]
    public sealed class ScoreConfig : ScriptableObject
    {
        [SerializeField] private float comboWindowSeconds = 5f;
        [SerializeField] private float multiplierPerKill = 0.1f;

        public float ComboWindowSeconds => comboWindowSeconds;
        public float MultiplierPerKill => multiplierPerKill;

        private void OnValidate()
        {
            comboWindowSeconds = Mathf.Max(float.Epsilon, comboWindowSeconds);
            multiplierPerKill = Mathf.Max(0f, multiplierPerKill);
        }
    }
}
