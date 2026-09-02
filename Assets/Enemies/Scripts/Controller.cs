using RPGame.Combat.Damage;
using RPGame.Core.Statistics;
using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    [RequireComponent(typeof(StatisticsController))]
    [RequireComponent(typeof(Detection))]
    [RequireComponent(typeof(Movement))]
    [RequireComponent(typeof(Attack))]
    [RequireComponent(typeof(EnemyTargetable))]
    [RequireComponent(typeof(DamageReceiver))]
    [RequireComponent(typeof(Death))]
    public sealed class Controller : MonoBehaviour
    {
        [SerializeField] private Detection detection;
        [SerializeField] private Movement movement;
        [SerializeField] private Config config;
        [SerializeField] private Attack attack;

        private IEnemyBehaviour behaviour;

        internal IEnemyBehaviour Behaviour => behaviour;

        private void Start()
        {
            CacheRequiredComponents();
            if (!TryCreateBehaviour(out behaviour))
            {
                enabled = false;
            }
        }

        private void Update()
        {
            behaviour?.Tick(Time.deltaTime);
        }

        internal void Tick()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            behaviour?.Tick(Time.deltaTime);
        }

        private void CacheRequiredComponents()
        {
            if (detection == null)
            {
                detection = GetComponent<Detection>();
            }

            if (movement == null)
            {
                movement = GetComponent<Movement>();
            }

            if (attack == null)
            {
                attack = GetComponent<Attack>();
            }
        }

        private bool TryCreateBehaviour(out IEnemyBehaviour createdBehaviour)
        {
            createdBehaviour = null;

            if (!HasRequiredComponents())
            {
                return false;
            }

            attack.SetConfig(config);

            if (config.BehaviourConfig == null)
            {
                Debug.LogError("Missing field behaviourConfig.", this);
                return false;
            }

            switch (config.BehaviourConfig)
            {
                case MeleeEnemyBehaviourConfig:
                    if (!TryGetAttack(AttackType.Melee, out IEnemyAttack meleeAttack))
                    {
                        return false;
                    }

                    createdBehaviour = new MeleeEnemyBehaviour(detection, movement, meleeAttack);
                    return true;

                case RangedEnemyBehaviourConfig rangedConfig:
                    if (!TryGetAttack(AttackType.StraightProjectile, out IEnemyAttack straightAttack)
                        || !TryGetAttack(AttackType.ParabolicProjectile, out IEnemyAttack parabolicAttack))
                    {
                        return false;
                    }

                    createdBehaviour = new RangedEnemyBehaviour(
                        detection,
                        movement,
                        rangedConfig,
                        straightAttack,
                        parabolicAttack);
                    return true;

                default:
                    Debug.LogError(
                        $"Unsupported behaviour config '{config.BehaviourConfig.GetType().Name}'.",
                        this);
                    return false;
            }
        }

        private bool HasRequiredComponents()
        {
            if (detection == null)
            {
                Debug.LogError("Missing field detection.", this);
                return false;
            }

            if (movement == null)
            {
                Debug.LogError("Missing field movement.", this);
                return false;
            }

            if (config == null)
            {
                Debug.LogError("Missing field config.", this);
                return false;
            }

            if (attack == null)
            {
                Debug.LogError("Missing field attack.", this);
                return false;
            }

            return true;
        }

        private bool TryGetAttack(AttackType type, out IEnemyAttack runtimeAttack)
        {
            if (attack.TryGetRuntimeAttack(type, out runtimeAttack))
            {
                return true;
            }

            Debug.LogError($"Attack '{type}' failed to initialize.", this);
            return false;
        }
    }
}
