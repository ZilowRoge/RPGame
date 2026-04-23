using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public abstract class DrawingReceiverBase : MonoBehaviour, IDrawingReceiver
    {
        public abstract void SubmitDrawing(Texture2D drawingTexture);
    }
}
