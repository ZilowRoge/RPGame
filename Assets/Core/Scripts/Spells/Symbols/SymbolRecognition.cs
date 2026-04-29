using System.IO;
using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public sealed class SymbolRecognition : DrawingReceiverBase
    {
        [SerializeField] private SymbolRecognizerBase recognizer;
        [SerializeField] private SymbolReceiverBase symbolReceiver;
        [SerializeField] private bool saveSubmittedSymbols;
        [SerializeField] private string savedSymbolsDirectoryName = "SubmittedSymbols";
        [SerializeField] private string unrecognizedDirectoryName = "Unrecognized";

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

            string labelDirectoryName = result.IsRecognized
                ? result.SymbolId.ToString()
                : unrecognizedDirectoryName;
            string rootDirectoryPath = Path.Combine(Application.persistentDataPath, savedSymbolsDirectoryName);
            string directoryPath = Path.Combine(rootDirectoryPath, labelDirectoryName);
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(
                directoryPath,
                $"symbol_{System.DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png");

            File.WriteAllBytes(filePath, drawingTexture.EncodeToPNG());
        }
    }
}
