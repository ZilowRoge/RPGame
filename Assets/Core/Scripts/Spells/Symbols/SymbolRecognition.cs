using System.IO;
using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public sealed class SymbolRecognition : DrawingReceiverBase
    {
        [SerializeField] private SymbolRecognizerBase recognizer;
        [SerializeField] private SymbolReceiverBase symbolReceiver;
        [SerializeField] private bool saveSubmittedSymbols;
        [SerializeField] private string rawSymbolsDirectoryName = "SubmittedSymbolsRaw";
        [SerializeField] private string preprocessedSymbolsDirectoryName = "SubmittedSymbols";

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
                result = SymbolRecognitionResult.NotRecognized;
            }

            if (saveSubmittedSymbols)
            {
                SaveSubmittedSymbol(drawingTexture, result);
            }

            symbolReceiver.ReceiveSymbol(result);
        }

        private void SaveSubmittedSymbol(Texture2D drawingTexture, SymbolRecognitionResult result)
        {
            if (drawingTexture == null)
            {
                return;
            }

            Texture2D preprocessedTexture = SymbolDrawingUtility.CreateWhiteForegroundTexture(drawingTexture);
            if (preprocessedTexture == null)
            {
                return;
            }

            string fileName = $"symbol_{System.DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png";
            string rawDirectoryPath = Path.Combine(Application.persistentDataPath, rawSymbolsDirectoryName);
            string preprocessedDirectoryPath = Path.Combine(Application.persistentDataPath, preprocessedSymbolsDirectoryName);
            Directory.CreateDirectory(rawDirectoryPath);
            Directory.CreateDirectory(preprocessedDirectoryPath);

            try
            {
                File.WriteAllBytes(Path.Combine(rawDirectoryPath, fileName), drawingTexture.EncodeToPNG());
                File.WriteAllBytes(Path.Combine(preprocessedDirectoryPath, fileName), preprocessedTexture.EncodeToPNG());
            }
            finally
            {
                Destroy(preprocessedTexture);
            }
        }
    }
}
