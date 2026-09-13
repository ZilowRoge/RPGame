using UnityEngine;

namespace RPGame.Core.Utility
{
    public sealed class DestroyOnAnimationEvent : MonoBehaviour
    {
        public void DestroySelf()
        {
            Destroy(gameObject);
        }
    }
}
