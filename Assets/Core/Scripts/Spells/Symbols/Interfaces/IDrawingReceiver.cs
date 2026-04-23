using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public interface IDrawingReceiver
    {
        void SubmitDrawing(Texture2D drawingTexture);
    }
}
