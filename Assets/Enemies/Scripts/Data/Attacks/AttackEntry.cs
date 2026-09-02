using System;
using UnityEngine;

namespace RPGame.Enemies
{
    [Serializable]
    public sealed class AttackEntry
    {
        [SerializeField] private AttackType type;
        [SerializeField] private AttackConfig config;

        public AttackType Type => type;
        public AttackConfig Config => config;
    }
}
