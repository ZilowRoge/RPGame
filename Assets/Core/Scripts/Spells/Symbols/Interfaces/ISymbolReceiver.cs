namespace RPGame.Core.Spells.Symbols
{
    public interface ISymbolReceiver
    {
        void ReceiveSymbol(SymbolRecognitionResult result);
    }
}
