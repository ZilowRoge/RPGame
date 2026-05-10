using UnityEngine;

namespace RPGame.UI.Jobs
{
    public readonly struct PerkTreeConnection
    {
        public PerkTreeConnection(Vector2 from, Vector2 to, PerkTreeConnectionState state)
        {
            From = from;
            To = to;
            State = state;
        }

        public Vector2 From { get; }
        public Vector2 To { get; }
        public PerkTreeConnectionState State { get; }
    }
}
