using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public interface ISymbolRecognizer
    {
        bool TryRecognize(Texture2D drawingTexture, out SymbolRecognitionResult result);
    }
}
