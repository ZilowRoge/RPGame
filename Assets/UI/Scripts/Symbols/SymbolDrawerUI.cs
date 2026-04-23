using System.Collections.Generic;
using RPGame.Core.Spells.Symbols;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGame.UI.Symbols
{
    public sealed class SymbolDrawerUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RawImage drawingImage;
        [SerializeField] private RawImage normalizedDebugImage;
        [SerializeField] private DrawingReceiverBase drawingReceiver;
        [SerializeField] private int textureSize = 256;
        [SerializeField] private Color backgroundColor = Color.clear;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private int brushRadius = 4;
        [SerializeField] private int normalizedTextureSize = 64;
        [SerializeField] private int normalizedPadding = 6;
        [SerializeField] private int normalizedLineRadius = 2;
        [SerializeField] private bool clearAfterSubmit = true;

        private Texture2D drawingTexture;
        private readonly List<Vector2Int> strokePoints = new List<Vector2Int>();
        private Vector2Int previousPixel;
        private Vector2Int minDrawingPixel;
        private Vector2Int maxDrawingPixel;
        private bool hasDrawingBounds;
        private bool isDrawing;

        private void Awake()
        {
            CreateDrawingTexture();
            Clear();
            SetDrawingImageActive(false);
        }

        private void Update()
        {
            SetDrawingImageActive(IsDrawModifierPressed() || isDrawing);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsDrawModifierPressed())
            {
                return;
            }

            isDrawing = TryGetPixel(eventData, out previousPixel);
            if (isDrawing)
            {
                ResetDrawingBounds();
                strokePoints.Clear();
                DrawBrush(previousPixel);
                IncludeStrokePoint(previousPixel);
                drawingTexture.Apply();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDrawing || !TryGetPixel(eventData, out Vector2Int currentPixel))
            {
                return;
            }

            DrawLine(previousPixel, currentPixel);
            previousPixel = currentPixel;
            drawingTexture.Apply();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !isDrawing)
            {
                return;
            }

            isDrawing = false;
            SubmitDrawing();
        }

        public void SubmitDrawing()
        {
            Texture2D submittedTexture = CreateNormalizedDrawingTexture();
            if (submittedTexture == null)
            {
                return;
            }

            if (normalizedDebugImage != null)
            {
                normalizedDebugImage.texture = submittedTexture;
            }

            if (drawingReceiver == null)
            {
                Debug.LogWarning("Symbol drawing was normalized, but drawingReceiver is not assigned.", this);
                return;
            }

            drawingReceiver.SubmitDrawing(submittedTexture);

            if (clearAfterSubmit)
            {
                Clear();
            }
        }

        public void Clear()
        {
            if (drawingTexture == null)
            {
                CreateDrawingTexture();
            }

            Color[] pixels = drawingTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = backgroundColor;
            }

            drawingTexture.SetPixels(pixels);
            drawingTexture.Apply();
            ResetDrawingBounds();
            strokePoints.Clear();
        }

        private void CreateDrawingTexture()
        {
            textureSize = Mathf.Max(16, textureSize);
            drawingTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            drawingTexture.wrapMode = TextureWrapMode.Clamp;
            drawingTexture.filterMode = FilterMode.Point;

            if (drawingImage != null)
            {
                drawingImage.texture = drawingTexture;
            }
        }

        private void SetDrawingImageActive(bool active)
        {
            if (drawingImage == null || drawingImage.gameObject.activeSelf == active)
            {
                return;
            }

            drawingImage.gameObject.SetActive(active);
        }

        private bool TryGetPixel(PointerEventData eventData, out Vector2Int pixel)
        {
            pixel = default;
            if (drawingImage == null)
            {
                return false;
            }

            RectTransform rectTransform = drawingImage.rectTransform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
            {
                return false;
            }

            Rect rect = rectTransform.rect;
            float normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            float normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

            if (normalizedX < 0f || normalizedX > 1f || normalizedY < 0f || normalizedY > 1f)
            {
                return false;
            }

            pixel = new Vector2Int(
                Mathf.RoundToInt(normalizedX * (textureSize - 1)),
                Mathf.RoundToInt(normalizedY * (textureSize - 1)));
            return true;
        }

        private void DrawLine(Vector2Int from, Vector2Int to)
        {
            int steps = Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
            if (steps == 0)
            {
                DrawBrush(to);
                IncludeStrokePoint(to);
                return;
            }

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector2Int pixel = new Vector2Int(
                    Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t)),
                    Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t)));
                DrawBrush(pixel);
                IncludeStrokePoint(pixel);
            }
        }

        private void DrawBrush(Vector2Int center)
        {
            int radius = Mathf.Max(1, brushRadius);
            int radiusSquared = radius * radius;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radiusSquared)
                    {
                        continue;
                    }

                    int pixelX = center.x + x;
                    int pixelY = center.y + y;
                    if (pixelX < 0 || pixelX >= textureSize || pixelY < 0 || pixelY >= textureSize)
                    {
                        continue;
                    }

                    drawingTexture.SetPixel(pixelX, pixelY, lineColor);
                }
            }
        }

        private Texture2D CreateNormalizedDrawingTexture()
        {
            if (!hasDrawingBounds || strokePoints.Count == 0)
            {
                return null;
            }

            int size = Mathf.Max(16, normalizedTextureSize);
            int padding = Mathf.Clamp(normalizedPadding, 0, size / 2 - 1);
            int sourceWidth = maxDrawingPixel.x - minDrawingPixel.x + 1;
            int sourceHeight = maxDrawingPixel.y - minDrawingPixel.y + 1;
            int drawableSize = Mathf.Max(1, size - padding * 2);
            float scale = Mathf.Min(drawableSize / (float)sourceWidth, drawableSize / (float)sourceHeight);
            float offsetX = (size - sourceWidth * scale) * 0.5f;
            float offsetY = (size - sourceHeight * scale) * 0.5f;

            Texture2D normalizedTexture = new Texture2D(size, size, drawingTexture.format, false);
            normalizedTexture.wrapMode = TextureWrapMode.Clamp;
            normalizedTexture.filterMode = FilterMode.Point;
            FillTexture(normalizedTexture, backgroundColor);

            Vector2 previousNormalizedPixel = MapToNormalizedPixel(strokePoints[0], scale, offsetX, offsetY);
            DrawNormalizedBrush(normalizedTexture, previousNormalizedPixel);

            for (int i = 1; i < strokePoints.Count; i++)
            {
                Vector2 normalizedPixel = MapToNormalizedPixel(strokePoints[i], scale, offsetX, offsetY);
                DrawNormalizedLine(normalizedTexture, previousNormalizedPixel, normalizedPixel);
                previousNormalizedPixel = normalizedPixel;
            }

            normalizedTexture.Apply();
            return normalizedTexture;
        }

        private Vector2 MapToNormalizedPixel(Vector2Int sourcePixel, float scale, float offsetX, float offsetY)
        {
            return new Vector2(
                (sourcePixel.x - minDrawingPixel.x + 0.5f) * scale + offsetX,
                (sourcePixel.y - minDrawingPixel.y + 0.5f) * scale + offsetY);
        }

        private void DrawNormalizedLine(Texture2D targetTexture, Vector2 from, Vector2 to)
        {
            int x0 = Mathf.RoundToInt(from.x);
            int y0 = Mathf.RoundToInt(from.y);
            int x1 = Mathf.RoundToInt(to.x);
            int y1 = Mathf.RoundToInt(to.y);
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx - dy;

            while (true)
            {
                DrawNormalizedBrush(targetTexture, new Vector2(x0, y0));
                if (x0 == x1 && y0 == y1)
                {
                    break;
                }

                int doubledError = error * 2;
                if (doubledError > -dy)
                {
                    error -= dy;
                    x0 += sx;
                }

                if (doubledError < dx)
                {
                    error += dx;
                    y0 += sy;
                }
            }
        }

        private void DrawNormalizedBrush(Texture2D targetTexture, Vector2 center)
        {
            int centerX = Mathf.RoundToInt(center.x);
            int centerY = Mathf.RoundToInt(center.y);
            int radius = Mathf.Max(1, normalizedLineRadius);
            int radiusSquared = radius * radius;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radiusSquared)
                    {
                        continue;
                    }

                    int pixelX = centerX + x;
                    int pixelY = centerY + y;
                    if (pixelX < 0 || pixelX >= targetTexture.width || pixelY < 0 || pixelY >= targetTexture.height)
                    {
                        continue;
                    }

                    targetTexture.SetPixel(pixelX, pixelY, lineColor);
                }
            }
        }

        private static void FillTexture(Texture2D targetTexture, Color color)
        {
            Color[] pixels = targetTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            targetTexture.SetPixels(pixels);
        }

        private void IncludeStrokePoint(Vector2Int pixel)
        {
            strokePoints.Add(pixel);
            if (!hasDrawingBounds)
            {
                minDrawingPixel = pixel;
                maxDrawingPixel = pixel;
                hasDrawingBounds = true;
                return;
            }

            minDrawingPixel = Vector2Int.Min(minDrawingPixel, pixel);
            maxDrawingPixel = Vector2Int.Max(maxDrawingPixel, pixel);
        }

        private void ResetDrawingBounds()
        {
            minDrawingPixel = Vector2Int.zero;
            maxDrawingPixel = Vector2Int.zero;
            hasDrawingBounds = false;
        }

        private static bool IsDrawModifierPressed()
        {
            return Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
        }

        private void OnValidate()
        {
            textureSize = Mathf.Max(16, textureSize);
            brushRadius = Mathf.Max(1, brushRadius);
            normalizedTextureSize = Mathf.Max(16, normalizedTextureSize);
            normalizedPadding = Mathf.Max(0, normalizedPadding);
            normalizedLineRadius = Mathf.Max(1, normalizedLineRadius);
        }
    }
}
