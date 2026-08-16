using UnityEngine;

namespace RPGame.Core.Statistics
{
    public sealed class DestroyOnDeath : MonoBehaviour
    {
        [SerializeField] private StatisticsController deathSource;

        private StatisticsController subscribedDeathSource;

        private void OnEnable()
        {
            SubscribeDeathSource();
        }

        private void OnDisable()
        {
            UnsubscribeDeathSource();
        }

        private void SubscribeDeathSource()
        {
            StatisticsController resolvedDeathSource = ResolveDeathSource();
            if (resolvedDeathSource == null)
            {
                return;
            }

            resolvedDeathSource.Died -= DestroyOwner;
            resolvedDeathSource.Died += DestroyOwner;
            subscribedDeathSource = resolvedDeathSource;
        }

        private void UnsubscribeDeathSource()
        {
            if (subscribedDeathSource != null)
            {
                subscribedDeathSource.Died -= DestroyOwner;
                subscribedDeathSource = null;
            }
        }

        private StatisticsController ResolveDeathSource()
        {
            if (deathSource == null)
            {
                deathSource = GetComponentInParent<StatisticsController>();
            }

            return deathSource;
        }

        private void DestroyOwner()
        {
            Destroy(gameObject);
        }
    }
}
