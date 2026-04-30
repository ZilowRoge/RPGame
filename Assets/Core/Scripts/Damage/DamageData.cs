using UnityEngine;

namespace RPGame.Core.Damage
{
    public readonly struct DamageData
    {
        public DamageData(float amount, GameObject source = null)
        {
            Amount = Mathf.Max(0f, amount);
            Source = source;
        }

        public float Amount { get; }
        public GameObject Source { get; }

        public bool HasDamage => Amount > 0f;
    }
}
