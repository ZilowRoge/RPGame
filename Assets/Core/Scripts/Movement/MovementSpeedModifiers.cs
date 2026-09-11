using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Movement
{
    public sealed class MovementSpeedModifiers
    {
        private readonly Dictionary<int, float> modifiers = new();
        private int nextModifierId;

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

        public int Add(float multiplier)
        {
            int modifierId = GetNextModifierId();
            modifiers.Add(modifierId, Mathf.Max(0f, multiplier));
            return modifierId;
        }

        public bool Remove(int modifierId)
        {
            return modifiers.Remove(modifierId);
        }

        private int GetNextModifierId()
        {
            for (int i = 0; i < int.MaxValue; i++)
            {
                if (nextModifierId == int.MaxValue)
                {
                    nextModifierId = 0;
                }

                nextModifierId++;
                if (!modifiers.ContainsKey(nextModifierId))
                {
                    return nextModifierId;
                }
            }

            return 0;
        }
    }
}
