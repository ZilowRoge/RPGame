using NUnit.Framework;
using RPGame.Core.Spells.Symbols;
using UnityEngine;

namespace RPGame.Core.Tests
{
    public sealed class SymbolDrawingUtilityTests
    {
        private Texture2D sourceTexture;

        [SetUp]
        public void SetUp()
        {
            sourceTexture = new Texture2D(
                SymbolDrawingConstants.DrawingTextureSize,
                SymbolDrawingConstants.DrawingTextureSize,
                TextureFormat.RGBA32,
                false);
            Fill(sourceTexture, Color.clear);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(sourceTexture);
        }

        [Test]
        public void CreateNormalizedTexture_WithoutBounds_ReturnsNull()
        {
            Texture2D normalizedTexture = SymbolDrawingUtility.CreateNormalizedTexture(
                sourceTexture,
                Vector2Int.zero,
                Vector2Int.zero,
                false);

            Assert.IsNull(normalizedTexture);
        }

        [Test]
        public void CreateNormalizedTexture_WithBounds_ReturnsTextureWithExpectedSize()
        {
            sourceTexture.SetPixel(10, 20, Color.red);
            sourceTexture.SetPixel(30, 40, Color.red);
            sourceTexture.Apply();

            Texture2D normalizedTexture = SymbolDrawingUtility.CreateNormalizedTexture(
                sourceTexture,
                new Vector2Int(10, 20),
                new Vector2Int(30, 40),
                true);

            Assert.NotNull(normalizedTexture);
            Assert.AreEqual(SymbolDrawingConstants.NormalizedTextureSize, normalizedTexture.width);
            Assert.AreEqual(SymbolDrawingConstants.NormalizedTextureSize, normalizedTexture.height);

            Object.DestroyImmediate(normalizedTexture);
        }

        [Test]
        public void CreateNormalizedTexture_PreservesTransparentBackground()
        {
            sourceTexture.SetPixel(110, 110, Color.red);
            sourceTexture.Apply();

            Texture2D normalizedTexture = SymbolDrawingUtility.CreateNormalizedTexture(
                sourceTexture,
                new Vector2Int(100, 100),
                new Vector2Int(120, 120),
                true);

            Color[] pixels = normalizedTexture.GetPixels();
            bool foundTransparentPixel = false;
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 0.0001f)
                {
                    continue;
                }

                foundTransparentPixel = true;
                break;
            }

            Assert.IsTrue(foundTransparentPixel);

            Object.DestroyImmediate(normalizedTexture);
        }

        private static void Fill(Texture2D texture, Color color)
        {
            Color[] pixels = texture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
        }
    }
}
