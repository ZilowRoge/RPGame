namespace RPGame.Core.Effects
{
    public abstract class StatusEffectDefinition : EffectDefinition
    {
        public virtual ReapplyPolicy ReapplyPolicy => ReapplyPolicy.Stack;

        public virtual bool CanApply(StatusEffectTarget target)
        {
            return true;
        }

        public virtual void OnApply(StatusEffectTarget target, StatusEffectInstance instance)
        {
        }

        public virtual void Tick(StatusEffectTarget target, float deltaTime)
        {
        }

        public virtual void OnRemove(StatusEffectTarget target, StatusEffectInstance instance)
        {
        }

        public virtual bool IsFinished(StatusEffectTarget target)
        {
            return false;
        }
    }
}
