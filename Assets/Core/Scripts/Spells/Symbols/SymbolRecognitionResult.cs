namespace RPGame.Core.Spells.Symbols
{
    public readonly struct SymbolRecognitionResult
    {
        public SymbolRecognitionResult(int symbolId, float confidence, bool isRecognized = true)
        {
            SymbolId = symbolId;
            Confidence = confidence;
            IsRecognized = isRecognized;
        }

        public int SymbolId { get; }
        public float Confidence { get; }
        public bool IsRecognized { get; }

        public static SymbolRecognitionResult NotRecognized => new SymbolRecognitionResult(-1, 0f, false);
    }
}
