namespace RPGame.Core.Damage
{
    public interface IDamageable
    {
        bool CanReceiveDamage { get; }

        DamageResult ApplyDamage(DamageData data);
    }
}
