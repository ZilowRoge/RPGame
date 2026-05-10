using System.Text;
using RPGame.Inventory.Data;
using RPGame.Inventory.Logic;
using RPGame.UI;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Inventory
{
    public sealed class ItemTooltipUI : TooltipUIBase
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;

        private readonly StringBuilder statsBuilder = new();

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

            ShowAt(screenPosition);
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

    }
}
