using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public sealed class TextureTemplateSymbolRecognizer : SymbolRecognizerBase
    {
        [SerializeField] private int mockedSymbolId = 6;
        [SerializeField, Range(0f, 1f)] private float mockedConfidence = 1f;

        public override bool TryRecognize(Texture2D drawingTexture, out SymbolRecognitionResult result)
        {
            result = new SymbolRecognitionResult(mockedSymbolId, mockedConfidence);
            return true;
        }

        private void OnValidate()
        {
            mockedConfidence = Mathf.Clamp01(mockedConfidence);
        }
    }
}
