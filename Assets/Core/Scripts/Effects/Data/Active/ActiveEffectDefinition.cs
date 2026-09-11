namespace RPGame.Core.Effects
{
    public abstract class ActiveEffectDefinition : EffectDefinition
    {
        public virtual ReapplyPolicy ReapplyPolicy => ReapplyPolicy.Stack;

        public virtual void OnApply(EffectTarget target)
        {
        }

        public virtual void Tick(EffectTarget target, float deltaTime)
        {
        }

        public virtual void OnRemove(EffectTarget target)
        {
        }

        public virtual bool IsFinished(EffectTarget target)
        {
            return false;
        }
    }
}
