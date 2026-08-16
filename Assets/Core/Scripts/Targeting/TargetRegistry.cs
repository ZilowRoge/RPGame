using System.Collections.Generic;

namespace RPGame.Core.Targeting
{
    public static class TargetRegistry
    {
        private static readonly List<ITargetable> targets = new();
        private static readonly HashSet<ITargetable> registeredTargets = new();

        public static IReadOnlyList<ITargetable> Targets => targets;
        public static int Count => targets.Count;

        internal static void Register(ITargetable targetable)
        {
            if (targetable == null || !registeredTargets.Add(targetable))
            {
                return;
            }

            targets.Add(targetable);
        }

        internal static void Unregister(ITargetable targetable)
        {
            if (targetable == null || !registeredTargets.Remove(targetable))
            {
                return;
            }

            targets.Remove(targetable);
        }

        internal static void Clear()
        {
            targets.Clear();
            registeredTargets.Clear();
        }
    }
}
