using UnityEngine;

namespace RPGame.Core.Spells.Symbols
{
    public static class SymbolDrawingUtility
    {
        public static Texture2D CreateWhiteForegroundTexture(Texture2D sourceTexture)
        {
            if (sourceTexture == null)
            {
                return null;
            }

            Texture2D processedTexture = new Texture2D(sourceTexture.width, sourceTexture.height, sourceTexture.format, false);
            processedTexture.wrapMode = sourceTexture.wrapMode;
            processedTexture.filterMode = sourceTexture.filterMode;

            Color[] pixels = sourceTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];
                if (pixel.a <= 0f)
                {
                    pixels[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                pixels[i] = new Color(1f, 1f, 1f, pixel.a);
            }

            processedTexture.SetPixels(pixels);
            processedTexture.Apply();
            return processedTexture;
        }

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

            int padding = Mathf.Max(0, SymbolDrawingConstants.PreNormalizationPadding);
            int paddedWidth = sourceWidth + (padding * 2);
            int paddedHeight = sourceHeight + (padding * 2);

            Texture2D paddedTexture = new Texture2D(paddedWidth, paddedHeight, sourceTexture.format, false);
            paddedTexture.wrapMode = TextureWrapMode.Clamp;
            paddedTexture.filterMode = FilterMode.Bilinear;
            FillTexture(paddedTexture, new Color(0f, 0f, 0f, 0f));
            paddedTexture.SetPixels(padding, padding, sourceWidth, sourceHeight, croppedTexture.GetPixels());
            paddedTexture.Apply();

            Texture2D normalizedTexture = new Texture2D(size, size, sourceTexture.format, false);
            normalizedTexture.wrapMode = TextureWrapMode.Clamp;
            normalizedTexture.filterMode = FilterMode.Bilinear;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = size > 1 ? x / (float)(size - 1) : 0f;
                    float v = size > 1 ? y / (float)(size - 1) : 0f;
                    normalizedTexture.SetPixel(x, y, paddedTexture.GetPixelBilinear(u, v));
                }
            }

            normalizedTexture.Apply();
            DestroyTexture(croppedTexture);
            DestroyTexture(paddedTexture);
            return normalizedTexture;
        }

        private static void DestroyTexture(Texture2D texture)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(texture);
                return;
            }

            Object.DestroyImmediate(texture);
        }

        private static void FillTexture(Texture2D texture, Color color)
        {
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
        }
    }
}
