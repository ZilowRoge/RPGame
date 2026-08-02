using RPGame.UI;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Statistics
{
    public sealed class AttributeCostTooltipUI : TooltipUIBase
    {
        [SerializeField] private TextMeshProUGUI costText;

        public void Show(int cost, Vector2 screenPosition)
        {
            if (costText != null)
            {
                costText.text = $"XP cost: {cost}";
            }

            ShowAt(screenPosition);
        }
    }
}
