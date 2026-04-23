using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public abstract class SymbolRecognizerBase : MonoBehaviour, ISymbolRecognizer
    {
        public abstract bool TryRecognize(Texture2D drawingTexture, out SymbolRecognitionResult result);
    }
}
