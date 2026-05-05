using System.Text;
using RPGame.Inventory.Data;
using RPGame.Inventory.Logic;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Inventory
{
    public sealed class ItemTooltipUI : MonoBehaviour
    {
        [SerializeField] private RectTransform tooltipTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Vector2 screenOffset = new(16f, -16f);
        [SerializeField] private Vector2 screenPadding = new(8f, 8f);

        private readonly StringBuilder statsBuilder = new();

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            Hide();
        }

        public void Show(ItemInstance item, Vector2 screenPosition)
        {
            if (item == null || item.Definition == null)
            {
                Hide();
                return;
            }

            ItemDefinition definition = item.Definition;
            transform.SetAsLastSibling();

            if (nameText != null)
            {
                nameText.text = definition.Name;
            }

            if (descriptionText != null)
            {
                SetTextSection(descriptionText, definition.Description);
            }

            if (statsText != null)
            {
                SetTextSection(statsText, BuildStats(definition));
            }

            SetPosition(screenPosition);
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private string BuildStats(ItemDefinition definition)
        {
            statsBuilder.Clear();

            for (int i = 0; i < definition.ItemTypeData.Count; i++)
            {
                ItemTypeData itemTypeData = definition.ItemTypeData[i];
                if (itemTypeData == null)
                {
                    continue;
                }

                string tooltip = itemTypeData.GetTooltip();
                if (string.IsNullOrWhiteSpace(tooltip))
                {
                    continue;
                }

                if (statsBuilder.Length > 0)
                {
                    statsBuilder.AppendLine();
                }

                statsBuilder.Append(tooltip);
            }

            return statsBuilder.ToString();
        }

        private static void SetTextSection(TextMeshProUGUI text, string value)
        {
            string displayValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
            text.text = displayValue;

            Transform parent = text.transform.parent;
            if (parent != null)
            {
                parent.gameObject.SetActive(!string.IsNullOrEmpty(displayValue));
            }
            else
            {
                text.gameObject.SetActive(!string.IsNullOrEmpty(displayValue));
            }
        }

        private void SetPosition(Vector2 screenPosition)
        {
            RectTransform rectTransform = tooltipTransform != null ? tooltipTransform : transform as RectTransform;
            if (rectTransform == null)
            {
                return;
            }

            Vector2 offset = screenOffset;
            offset.y -= rectTransform.rect.height * (1f - rectTransform.pivot.y);
            rectTransform.position = ClampToScreen(screenPosition + offset, rectTransform);
        }

        private Vector2 ClampToScreen(Vector2 position, RectTransform rectTransform)
        {
            Rect rect = rectTransform.rect;
            Vector2 pivot = rectTransform.pivot;

            float minX = screenPadding.x + rect.width * pivot.x;
            float maxX = Screen.width - screenPadding.x - rect.width * (1f - pivot.x);
            float minY = screenPadding.y + rect.height * pivot.y;
            float maxY = Screen.height - screenPadding.y - rect.height * (1f - pivot.y);

            if (minX > maxX)
            {
                position.x = Screen.width * 0.5f;
            }
            else
            {
                position.x = Mathf.Clamp(position.x, minX, maxX);
            }

            if (minY > maxY)
            {
                position.y = Screen.height * 0.5f;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, minY, maxY);
            }

            return position;
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}
