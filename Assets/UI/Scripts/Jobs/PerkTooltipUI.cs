using RPGame.Progression;
using RPGame.UI;
using TMPro;
using UnityEngine;

namespace RPGame.UI.Jobs
{
    public sealed class PerkTooltipUI : TooltipUIBase
    {
        private const string NoBonusesText = "No bonuses";

        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI bonusesText;

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

            ShowAt(screenPosition);
        }
    }
}
