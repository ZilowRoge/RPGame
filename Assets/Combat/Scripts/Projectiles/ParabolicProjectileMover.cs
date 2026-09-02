using System;
using UnityEngine;

namespace RPGame.Combat.Projectiles
{
    public enum ParabolicProjectilePhase
    {
        Ascending,
        Apex,
        Descending,
        Complete
    }

    public sealed class ParabolicProjectileMover : ProjectileMover
    {
        private const float MinDuration = 0.001f;

        private Vector3 startPoint;
        private Vector3 controlPoint;
        private Vector3 apexPoint;
        private Vector3 impactPoint;
        private float ascentDuration;
        private float apexPauseDuration;
        private float descentDuration;
        private float phaseElapsed;

        public ParabolicProjectilePhase Phase { get; private set; }
        public Vector3 ApexPoint => apexPoint;
        public Vector3 ImpactPoint => impactPoint;
        public float CurrentSpeed { get; private set; }
        public bool HasReachedApex { get; private set; }
        public bool ReachedApexThisTick { get; private set; }
        public bool IsComplete => Phase == ParabolicProjectilePhase.Complete;
        private bool HasApexPause => apexPauseDuration > 0f;

        public event Action ApexReached;

        public void InitializeTrajectory(
            Vector3 start,
            Vector3 impact,
            float arcHeight,
            float ascentDuration,
            float apexPauseDuration,
            float descentDuration)
        {
            startPoint = start;
            impactPoint = impact;
            apexPoint = Vector3.Lerp(startPoint, impactPoint, 0.5f) + Vector3.up * Mathf.Max(0f, arcHeight);
            controlPoint = 2f * apexPoint - 0.5f * (startPoint + impactPoint);
            this.ascentDuration = Mathf.Max(MinDuration, ascentDuration);
            this.apexPauseDuration = Mathf.Max(0f, apexPauseDuration);
            this.descentDuration = Mathf.Max(MinDuration, descentDuration);
            phaseElapsed = 0f;
            CurrentSpeed = 0f;
            HasReachedApex = false;
            ReachedApexThisTick = false;
            Phase = ParabolicProjectilePhase.Ascending;
            transform.position = startPoint;
        }

        public override void Tick(float deltaTime)
        {
            ReachedApexThisTick = false;
            if (Phase == ParabolicProjectilePhase.Complete)
            {
                CurrentSpeed = 0f;
                return;
            }

            float remainingDeltaTime = Mathf.Max(0f, deltaTime);
            while (remainingDeltaTime > 0f && Phase != ParabolicProjectilePhase.Complete)
            {
                if (Phase == ParabolicProjectilePhase.Ascending)
                {
                    TickAscending(ref remainingDeltaTime);
                }
                else if (Phase == ParabolicProjectilePhase.Apex)
                {
                    TickApex(ref remainingDeltaTime);
                }
                else
                {
                    TickDescending(ref remainingDeltaTime);
                }
            }
        }

        private void TickAscending(ref float remainingDeltaTime)
        {
            float step = Mathf.Min(remainingDeltaTime, ascentDuration - phaseElapsed);
            phaseElapsed += step;
            remainingDeltaTime -= step;

            float localProgress = Mathf.Clamp01(phaseElapsed / ascentDuration);
            float easedProgress = HasApexPause ? SmoothStep(localProgress) : localProgress;
            float curveProgress = easedProgress * 0.5f;
            transform.position = localProgress >= 1f ? apexPoint : EvaluateCurve(curveProgress);
            CurrentSpeed = localProgress >= 1f
                ? GetDescentStartSpeed()
                : EvaluateSpeed(curveProgress, GetAscentCurveProgressPerSecond(localProgress));

            if (localProgress < 1f)
            {
                return;
            }

            transform.position = apexPoint;
            CurrentSpeed = GetDescentStartSpeed();
            HasReachedApex = true;
            ReachedApexThisTick = true;
            phaseElapsed = 0f;
            Phase = HasApexPause
                ? ParabolicProjectilePhase.Apex
                : ParabolicProjectilePhase.Descending;
            ApexReached?.Invoke();
        }

        private void TickApex(ref float remainingDeltaTime)
        {
            float step = Mathf.Min(remainingDeltaTime, apexPauseDuration - phaseElapsed);
            phaseElapsed += step;
            remainingDeltaTime -= step;
            transform.position = apexPoint;
            CurrentSpeed = 0f;

            if (phaseElapsed < apexPauseDuration)
            {
                return;
            }

            Phase = ParabolicProjectilePhase.Descending;
            phaseElapsed = 0f;
        }

        private void TickDescending(ref float remainingDeltaTime)
        {
            float step = Mathf.Min(remainingDeltaTime, descentDuration - phaseElapsed);
            phaseElapsed += step;
            remainingDeltaTime -= step;

            float localProgress = Mathf.Clamp01(phaseElapsed / descentDuration);
            float easedProgress = HasApexPause ? localProgress * localProgress : localProgress;
            float curveProgress = 0.5f + easedProgress * 0.5f;
            transform.position = localProgress >= 1f ? impactPoint : EvaluateCurve(curveProgress);
            CurrentSpeed = EvaluateSpeed(curveProgress, GetDescentCurveProgressPerSecond(localProgress));

            if (localProgress < 1f)
            {
                return;
            }

            transform.position = impactPoint;
            Phase = ParabolicProjectilePhase.Complete;
        }

        private Vector3 EvaluateCurve(float progress)
        {
            float inverseProgress = 1f - progress;
            return inverseProgress * inverseProgress * startPoint
                + 2f * inverseProgress * progress * controlPoint
                + progress * progress * impactPoint;
        }

        private Vector3 EvaluateDerivative(float progress)
        {
            return 2f * (1f - progress) * (controlPoint - startPoint)
                + 2f * progress * (impactPoint - controlPoint);
        }

        private float EvaluateSpeed(float curveProgress, float curveProgressPerSecond)
        {
            return EvaluateDerivative(curveProgress).magnitude * curveProgressPerSecond;
        }

        private float GetDescentStartSpeed()
        {
            return HasApexPause ? 0f : EvaluateSpeed(0.5f, GetDescentCurveProgressPerSecond(0f));
        }

        private float GetAscentCurveProgressPerSecond(float localProgress)
        {
            return HasApexPause ? SmoothStepDerivative(localProgress) * 0.5f / ascentDuration : 0.5f / ascentDuration;
        }

        private float GetDescentCurveProgressPerSecond(float localProgress)
        {
            return HasApexPause ? localProgress / descentDuration : 0.5f / descentDuration;
        }

        private static float SmoothStep(float progress)
        {
            return progress * progress * (3f - 2f * progress);
        }

        private static float SmoothStepDerivative(float progress)
        {
            return 6f * progress * (1f - progress);
        }
    }
}
