using UnityEngine;

namespace RPGame.Core.Statuses
{
    public readonly struct StatusContext
    {
        public StatusContext(StatusSourceId sourceId, GameObject source)
        {
            SourceId = sourceId;
            Source = source;
        }

        public StatusSourceId SourceId { get; }
        public GameObject Source { get; }
    }
}
