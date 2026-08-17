using UnityEngine;

namespace RPGame.Core.Targeting
{
    public interface ITargetable
    {
        Transform TargetPoint { get; }
    }
}
