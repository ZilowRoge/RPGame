using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGame.Core.Statistics
{
    public abstract class StatisticsDataProviderBase : MonoBehaviour
    {
        public event Action Changed;

        public abstract IReadOnlyList<DataEntry> GetStatistics();

        protected void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
