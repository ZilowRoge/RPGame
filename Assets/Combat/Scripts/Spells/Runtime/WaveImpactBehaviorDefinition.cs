using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(
        fileName = "WaveImpactBehaviorDefinition",
        menuName = "RPGame/Progression/Runtime Behaviors/Wave Impact")]
    public sealed class WaveImpactBehaviorDefinition : RuntimeSpellBehaviorDefinition
    {
        [SerializeField, Min(0f)] private float impactDamage = 1f;

        public override bool TryCreate(
            Spell spell,
            GameObject casterObject,
            out IRuntimeSpellBehavior behavior)
        {
            behavior = null;
            if (spell is not WaveSpell)
            {
                return false;
            }

            behavior = new KnockbackDamageOnImpactCollisionBehavior(impactDamage, casterObject);
            return true;
        }

        private void OnValidate()
        {
            impactDamage = Mathf.Max(0f, impactDamage);
        }
    }
}
