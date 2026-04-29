using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public static class SymbolDrawingUtility
    {
        public static Texture2D CreateNormalizedTexture(
            Texture2D sourceTexture,
            Vector2Int minPixel,
            Vector2Int maxPixel,
            bool hasDrawingBounds)
        {
            if (!hasDrawingBounds || sourceTexture == null)
            {
                return null;
            }

            int size = SymbolDrawingConstants.NormalizedTextureSize;
            int sourceWidth = maxPixel.x - minPixel.x + 1;
            int sourceHeight = maxPixel.y - minPixel.y + 1;
            if (sourceWidth <= 0 || sourceHeight <= 0)
            {
                return new Texture2D(size, size, sourceTexture.format, false);
            }

            Texture2D croppedTexture = new Texture2D(sourceWidth, sourceHeight, sourceTexture.format, false);
            croppedTexture.wrapMode = TextureWrapMode.Clamp;
            croppedTexture.filterMode = FilterMode.Bilinear;
            croppedTexture.SetPixels(sourceTexture.GetPixels(minPixel.x, minPixel.y, sourceWidth, sourceHeight));
            croppedTexture.Apply();

            Texture2D normalizedTexture = new Texture2D(size, size, sourceTexture.format, false);
            normalizedTexture.wrapMode = TextureWrapMode.Clamp;
            normalizedTexture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = size > 1 ? x / (float)(size - 1) : 0f;
                    float v = size > 1 ? y / (float)(size - 1) : 0f;
                    normalizedTexture.SetPixel(x, y, croppedTexture.GetPixelBilinear(u, v));
                }
            }

            normalizedTexture.Apply();
            Object.Destroy(croppedTexture);
            return normalizedTexture;
        }
    }
}
