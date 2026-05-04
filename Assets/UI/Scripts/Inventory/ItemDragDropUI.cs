using RPGame.Core.Inventory;
using RPGame.Core.Inventory.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace RPGame.UI.Inventory
{
    public sealed class ItemDragDropUI : MonoBehaviour
    {
        [SerializeField] private RectTransform dragIconTransform;
        [SerializeField] private Image dragIconImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private ItemSlotReference source;
        private bool hasSource;
        private bool isDragging;

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            if (dragIconImage != null)
            {
                dragIconImage.raycastTarget = false;
            }

            Hide();
        }

        public void BeginDrag(ItemSlotReference source, ItemInstance item, Vector2 screenPosition)
        {
            if (item == null || item.Definition == null)
            {
                return;
            }

            this.source = source;
            hasSource = true;
            isDragging = true;
            transform.SetAsLastSibling();

            if (dragIconImage != null)
            {
                dragIconImage.enabled = item.Definition.Icon != null;
                dragIconImage.sprite = item.Definition.Icon;
            }

            SetPosition(screenPosition);
            Show();
        }

        public void Move(Vector2 screenPosition)
        {
            if (!isDragging)
            {
                return;
            }

            SetPosition(screenPosition);
        }

        public bool Drop(ItemSlotReference target, ItemManagementController controller)
        {
            if (!hasSource || controller == null)
            {
                EndDrag();
                return false;
            }

            bool moved = controller.MoveItem(source, target);
            EndDrag();
            return moved;
        }

        public void EndDrag()
        {
            hasSource = false;
            isDragging = false;
            Hide();
        }

        private void SetPosition(Vector2 screenPosition)
        {
            RectTransform rectTransform = dragIconTransform != null ? dragIconTransform : transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.position = screenPosition;
            }
        }

        private void Show()
        {
            canvasGroup.alpha = 1f;
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
        }
    }
}
