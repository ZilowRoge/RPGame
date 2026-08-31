using System.Collections.Generic;

namespace RPGame.Core.Targeting
{
    public static class TargetRegistry
    {
        private static readonly List<EnemyTargetable> enemyTargets = new();
        private static readonly HashSet<EnemyTargetable> registeredEnemyTargets = new();
        private static readonly List<PlayerTargetable> playerTargets = new();
        private static readonly HashSet<PlayerTargetable> registeredPlayerTargets = new();

        public static IReadOnlyList<EnemyTargetable> EnemyTargets => enemyTargets;
        public static IReadOnlyList<PlayerTargetable> PlayerTargets => playerTargets;
        public static int EnemyTargetCount => enemyTargets.Count;
        public static int PlayerTargetCount => playerTargets.Count;

        public static void RegisterTarget(EnemyTargetable targetable)
        {
            Register(targetable, registeredEnemyTargets, enemyTargets);
        }

        public static void UnregisterTarget(EnemyTargetable targetable)
        {
            Unregister(targetable, registeredEnemyTargets, enemyTargets);
        }

        public static void RegisterTarget(PlayerTargetable targetable)
        {
            Register(targetable, registeredPlayerTargets, playerTargets);
        }

        public static void UnregisterTarget(PlayerTargetable targetable)
        {
            Unregister(targetable, registeredPlayerTargets, playerTargets);
        }

        internal static void Clear()
        {
            enemyTargets.Clear();
            registeredEnemyTargets.Clear();
            playerTargets.Clear();
            registeredPlayerTargets.Clear();
        }

        private static void Register<TTarget>(
            TTarget targetable,
            HashSet<TTarget> registeredTargets,
            List<TTarget> targets)
            where TTarget : Targetable
        {
            if (targetable == null || !registeredTargets.Add(targetable))
            {
                return;
            }

            targets.Add(targetable);
        }

        private static void Unregister<TTarget>(
            TTarget targetable,
            HashSet<TTarget> registeredTargets,
            List<TTarget> targets)
            where TTarget : Targetable
        {
            if (targetable == null || !registeredTargets.Remove(targetable))
            {
                return;
            }

            targets.Remove(targetable);
        }
    }
}
