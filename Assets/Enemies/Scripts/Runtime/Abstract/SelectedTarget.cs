using RPGame.Core.Targeting;
using UnityEngine;

namespace RPGame.Enemies
{
    public readonly struct SelectedTarget
    {
        public SelectedTarget(ITargetable targetable, Vector3 position)
        {
            Targetable = targetable;
            Position = position;
        }

        public ITargetable Targetable { get; }
        public Vector3 Position { get; }
        public bool IsValid => Targetable != null;
    }
}
