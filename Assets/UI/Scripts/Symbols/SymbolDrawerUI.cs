using System.IO;
using RPGame.Core.Spells.Symbols;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RPGame.UI.Symbols
{
    public sealed class SymbolDrawerUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int DefaultTextureSize = 1024;
        private const int NormalizedTextureSize = 64;
        private const int BrushRadius = 2;

        [SerializeField] private RawImage drawingImage;
        [SerializeField] private RawImage normalizedDebugImage;
        [SerializeField] private DrawingReceiverBase drawingReceiver;
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private bool clearAfterSubmit = true;
        [SerializeField] private bool saveSubmittedSymbols;

        private Texture2D drawingTexture;
        private Vector2Int previousPixel;
        private Vector2Int minDrawingPixel;
        private Vector2Int maxDrawingPixel;
        private bool hasDrawingBounds;
        private bool isDrawing;

        private static Color BackgroundColor => Color.clear;

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
                DrawBrush(previousPixel);
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

            Debug.Log(
                $"Submitting symbol with textureSize={DefaultTextureSize}, brushRadius={BrushRadius}, backgroundColor={BackgroundColor}, lineColor={lineColor}, normalizedTextureSize={NormalizedTextureSize}.",
                this);

            if (normalizedDebugImage != null)
            {
                normalizedDebugImage.texture = submittedTexture;
            }

            if (saveSubmittedSymbols)
            {
                SaveSubmittedSymbol(submittedTexture);
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
                pixels[i] = BackgroundColor;
            }

            drawingTexture.SetPixels(pixels);
            drawingTexture.Apply();
            ResetDrawingBounds();
        }

        private void CreateDrawingTexture()
        {
            drawingTexture = new Texture2D(DefaultTextureSize, DefaultTextureSize, TextureFormat.RGBA32, false);
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
                Mathf.Clamp((int)(normalizedX * DefaultTextureSize), 0, DefaultTextureSize - 1),
                Mathf.Clamp((int)(normalizedY * DefaultTextureSize), 0, DefaultTextureSize - 1));
            return true;
        }

        private void DrawLine(Vector2Int from, Vector2Int to)
        {
            int x0 = from.x;
            int y0 = from.y;
            int x1 = to.x;
            int y1 = to.y;
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx - dy;

            while (true)
            {
                DrawBrush(new Vector2Int(x0, y0));
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

        private void DrawBrush(Vector2Int center)
        {
            int radius = BrushRadius;
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
                    if (pixelX < 0 || pixelX >= DefaultTextureSize || pixelY < 0 || pixelY >= DefaultTextureSize)
                    {
                        continue;
                    }

                    drawingTexture.SetPixel(pixelX, pixelY, lineColor);
                    IncludePixelInBounds(pixelX, pixelY);
                }
            }
        }

        private Texture2D CreateNormalizedDrawingTexture()
        {
            if (!hasDrawingBounds)
            {
                return null;
            }

            int size = NormalizedTextureSize;
            int sourceWidth = maxDrawingPixel.x - minDrawingPixel.x + 1;
            int sourceHeight = maxDrawingPixel.y - minDrawingPixel.y + 1;
            if (sourceWidth <= 0 || sourceHeight <= 0)
            {
                return new Texture2D(size, size, drawingTexture.format, false);
            }

            Texture2D croppedTexture = new Texture2D(sourceWidth, sourceHeight, drawingTexture.format, false);
            croppedTexture.wrapMode = TextureWrapMode.Clamp;
            croppedTexture.filterMode = FilterMode.Bilinear;
            croppedTexture.SetPixels(drawingTexture.GetPixels(minDrawingPixel.x, minDrawingPixel.y, sourceWidth, sourceHeight));
            croppedTexture.Apply();

            Texture2D normalizedTexture = new Texture2D(size, size, drawingTexture.format, false);
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
            Destroy(croppedTexture);
            return normalizedTexture;
        }

        private void SaveSubmittedSymbol(Texture2D submittedTexture)
        {
            if (submittedTexture == null)
            {
                return;
            }

            string directoryPath = Path.Combine(Application.persistentDataPath, "SubmittedSymbols");
            Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(
                directoryPath,
                $"symbol_{System.DateTime.UtcNow:yyyyMMdd_HHmmssfff}.png");

            File.WriteAllBytes(filePath, submittedTexture.EncodeToPNG());
            Debug.Log($"Saved submitted symbol to '{filePath}'.", this);
        }

        private void IncludePixelInBounds(int pixelX, int pixelY)
        {
            if (!hasDrawingBounds)
            {
                minDrawingPixel = new Vector2Int(pixelX, pixelY);
                maxDrawingPixel = new Vector2Int(pixelX, pixelY);
                hasDrawingBounds = true;
                return;
            }

            minDrawingPixel = Vector2Int.Min(minDrawingPixel, new Vector2Int(pixelX, pixelY));
            maxDrawingPixel = Vector2Int.Max(maxDrawingPixel, new Vector2Int(pixelX, pixelY));
        }

        private void ResetDrawingBounds()
        {
            minDrawingPixel = new Vector2Int(DefaultTextureSize, DefaultTextureSize);
            maxDrawingPixel = Vector2Int.zero;
            hasDrawingBounds = false;
        }

        private static bool IsDrawModifierPressed()
        {
            return Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed;
        }

        private void OnValidate()
        {
        }
    }
}
