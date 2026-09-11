using UnityEngine;

namespace RPGame.Core.Effects
{
    public sealed class StatusController : MonoBehaviour, IStatusController
    {
        private int stunSourceCount;

        public bool IsStunned => stunSourceCount > 0;

        public void BeginStun()
        {
            stunSourceCount++;
        }

        public void EndStun()
        {
            stunSourceCount = Mathf.Max(0, stunSourceCount - 1);
        }
    }
}
