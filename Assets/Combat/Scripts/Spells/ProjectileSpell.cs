using RPGame.Combat.Projectiles;
using RPGame.Core.Spells;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    [CreateAssetMenu(fileName = "ProjectileSpell", menuName = "RPGame/Spells/Projectile Spell")]
    public sealed class ProjectileSpell : Spell
    {
        public override void OnCast(CasterData casterData)
        {
            if (SpellPrefab == null)
            {
                Debug.LogWarning($"{name} cannot cast because spell prefab is missing.", this);
                return;
            }

            Transform castOrigin = casterData.CastOrigin;
            Vector3 position = castOrigin != null ? castOrigin.position : Vector3.zero;
            Quaternion rotation = castOrigin != null ? castOrigin.rotation : Quaternion.identity;

            GameObject projectileObject = Instantiate(SpellPrefab, position, rotation);
            if (!projectileObject.TryGetComponent(out ProjectileController projectile))
            {
                Debug.LogWarning($"{name} spawned a prefab without {nameof(ProjectileController)}.", projectileObject);
                Destroy(projectileObject);
                return;
            }

            projectile.Initialize(casterData);
        }
    }
}
