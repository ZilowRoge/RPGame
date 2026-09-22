using System;
using System.Collections.Generic;
using RPGame.Core.Spells;
using RPGame.Core.Statuses;
using UnityEngine;

namespace RPGame.Combat.Spells
{
    public static class SpellBehaviorPipelineExecutor
    {
        public static bool Resolve(
            CasterData casterData,
            GameObject target,
            ISpellResolveBehavior resolveBehavior)
        {
            if (target == null || resolveBehavior == null)
            {
                return false;
            }

            IStatusReceiver statusReceiver = target.GetComponentInParent<IStatusReceiver>();
            SpellBehaviorContext context = new(
                target,
                casterData.CasterObject,
                casterData.SpellId,
                statusReceiver);

            ExecuteTargetPhase(casterData, SpellBehaviorPhase.PreResolve, context);
            ExecuteMarkConsumers(casterData, context);

            if (resolveBehavior.Phase != SpellBehaviorPhase.Resolve
                || !resolveBehavior.Resolve(context))
            {
                return false;
            }

            ExecuteTargetPhase(casterData, SpellBehaviorPhase.PostResolve, context);
            ExecuteTargetPhase(casterData, SpellBehaviorPhase.Effect, context);
            RecordMarkProgress(context);
            return true;
        }

        private static void ExecuteTargetPhase(
            CasterData casterData,
            SpellBehaviorPhase phase,
            SpellBehaviorContext context)
        {
            ExecutePhase(
                casterData.RuntimeBehaviors,
                phase,
                behavior => behavior.Execute(context));
        }

        private static void ExecuteMarkConsumers(
            CasterData casterData,
            SpellBehaviorContext context)
        {
            IReadOnlyList<IRuntimeSpellBehavior> behaviors = casterData.RuntimeBehaviors;
            if (behaviors == null)
            {
                return;
            }

            for (int behaviorIndex = 0; behaviorIndex < behaviors.Count; behaviorIndex++)
            {
                if (behaviors[behaviorIndex] is IMarkConsumer markConsumer)
                {
                    markConsumer.TryConsumeMark(context);
                }
            }
        }

        private static void ExecutePhase(
            IReadOnlyList<IRuntimeSpellBehavior> behaviors,
            SpellBehaviorPhase phase,
            Action<ISpellBehavior> execute)
        {
            if (behaviors == null || execute == null)
            {
                return;
            }

            if (!TryGetMaxPriority(behaviors, phase, out int maxPriority))
            {
                return;
            }

            for (int priority = 0; priority <= maxPriority; priority++)
            {
                for (int behaviorIndex = 0; behaviorIndex < behaviors.Count; behaviorIndex++)
                {
                    IRuntimeSpellBehavior runtimeBehavior = behaviors[behaviorIndex];
                    if (runtimeBehavior is ISpellBehavior behavior
                        && behavior.Phase == phase
                        && behavior.Priority == priority)
                    {
                        execute(behavior);
                    }
                }
            }
        }

        private static bool TryGetMaxPriority(
            IReadOnlyList<IRuntimeSpellBehavior> behaviors,
            SpellBehaviorPhase phase,
            out int maxPriority)
        {
            maxPriority = -1;

            for (int behaviorIndex = 0; behaviorIndex < behaviors.Count; behaviorIndex++)
            {
                if (behaviors[behaviorIndex] is not ISpellBehavior behavior
                    || behavior.Phase != phase)
                {
                    continue;
                }

                if (behavior.Priority < 0)
                {
                    throw new InvalidOperationException(
                        $"Spell behavior priority cannot be negative for phase {phase}.");
                }

                for (int nextIndex = behaviorIndex + 1; nextIndex < behaviors.Count; nextIndex++)
                {
                    if (behaviors[nextIndex] is ISpellBehavior nextBehavior
                        && nextBehavior.Phase == phase
                        && nextBehavior.Priority == behavior.Priority)
                    {
                        throw new InvalidOperationException(
                            $"Duplicate spell behavior priority {behavior.Priority} for phase {phase}.");
                    }
                }

                if (behavior.Priority > maxPriority)
                {
                    maxPriority = behavior.Priority;
                }
            }

            return maxPriority >= 0;
        }

        private static void RecordMarkProgress(SpellBehaviorContext context)
        {
            MarkProgressReceiver progressReceiver =
                context.Target.GetComponentInParent<MarkProgressReceiver>();
            if (progressReceiver == null)
            {
                return;
            }

            progressReceiver.RecordSuccessfulSpell(
                context.SpellId,
                new StatusContext(new StatusSourceId(context.SpellId.ToString()), context.Source));
        }
    }
}
