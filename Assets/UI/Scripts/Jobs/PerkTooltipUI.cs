using RPGame.Progression;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Jobs
{
    public sealed class PerkTooltipUI : MonoBehaviour
    {
        private const string NoBonusesText = "No bonuses";

        [SerializeField] private RectTransform tooltipTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI bonusesText;
        [SerializeField] private Vector2 screenOffset = new(16f, -16f);
        [SerializeField] private Vector2 screenPadding = new(8f, 8f);

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            Hide();
        }

        public void Show(PerkDefinition perk, Vector2 screenPosition)
        {
            if (perk == null)
            {
                Hide();
                return;
            }

            transform.SetAsLastSibling();

            if (nameText != null)
            {
                nameText.text = perk.DisplayName;
            }

            if (descriptionText != null)
            {
                SetTextSection(descriptionText, perk.Description);
            }

            if (bonusesText != null)
            {
                SetTextSection(bonusesText, NoBonusesText);
            }

            SetPosition(screenPosition);
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
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

            position.x = minX > maxX ? Screen.width * 0.5f : Mathf.Clamp(position.x, minX, maxX);
            position.y = minY > maxY ? Screen.height * 0.5f : Mathf.Clamp(position.y, minY, maxY);

            return position;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
