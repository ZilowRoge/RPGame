using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public abstract class SymbolReceiverBase : MonoBehaviour, ISymbolReceiver
    {
        public abstract void ReceiveSymbol(SymbolRecognitionResult result);
    }
}
