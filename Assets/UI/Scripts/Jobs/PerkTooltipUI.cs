using System.Text;
using RPGame.Core.Effects;
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
                SetTextSection(bonusesText, GetBonusesText(perk));
            }

            ShowAt(screenPosition);
        }

        private static string GetBonusesText(PerkDefinition perk)
        {
            if (perk.Effects.Count == 0)
            {
                return NoBonusesText;
            }

            StringBuilder builder = new StringBuilder();
            foreach (EffectDefinition effect in perk.Effects)
            {
                if (effect == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(effect.ToString());
            }

            return builder.Length > 0 ? builder.ToString() : NoBonusesText;
        }
    }
}
