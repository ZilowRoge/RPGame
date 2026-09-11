using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Movement
{
    public sealed class MovementSpeedModifiers
    {
        private readonly Dictionary<IModifierSource, float> modifiers = new();

        public float Multiplier
        {
            get
            {
                float multiplier = 1f;
                foreach (float modifier in modifiers.Values)
                {
                    multiplier *= modifier;
                }

                return multiplier;
            }
        }

        public void Add(IModifierSource source, float multiplier)
        {
            if (source == null)
            {
                return;
            }

            modifiers[source] = Mathf.Max(0f, multiplier);
        }

        public bool Remove(IModifierSource source)
        {
            return source != null && modifiers.Remove(source);
        }
    }
}
