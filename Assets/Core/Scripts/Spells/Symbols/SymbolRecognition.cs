using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public sealed class SymbolRecognition : DrawingReceiverBase
    {
        [SerializeField] private SymbolRecognizerBase recognizer;
        [SerializeField] private SymbolReceiverBase symbolReceiver;

        public override void SubmitDrawing(Texture2D drawingTexture)
        {
            if (recognizer == null)
            {
                Debug.LogWarning("SymbolRecognition cannot recognize drawing because recognizer is not assigned.", this);
                return;
            }

            if (symbolReceiver == null)
            {
                Debug.LogWarning("SymbolRecognition pipeline stopped because symbolReceiver is not assigned.", this);
                return;
            }

            if (!recognizer.TryRecognize(drawingTexture, out SymbolRecognitionResult result))
            {
                Debug.LogWarning("Didn't recognized the symbol.", this);
                result = SymbolRecognitionResult.NotRecognized;
            }

            symbolReceiver.ReceiveSymbol(result);
        }
    }
}
