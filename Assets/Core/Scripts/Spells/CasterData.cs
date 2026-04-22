using UnityEngine;

namespace RPGame.Core.Spells
{
    public readonly struct CasterData
    {
        public CasterData(GameObject casterObject, Transform castOrigin, Transform target)
        {
            CasterObject = casterObject;
            CastOrigin = castOrigin;
            Target = target;
        }

        public GameObject CasterObject { get; }
        public Transform CastOrigin { get; }
        public Transform Target { get; }
    }
}
