using TMPro;
using UnityEngine;

namespace RPGame.UI.Statistics
{
    public sealed class StatisticRecordUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private TMP_Text valueText;

        private void Awake()
        {
            ResolveReferences();
        }

        public void SetText(string label, string value)
        {
            ResolveReferences();

            if (labelText != null)
            {
                labelText.text = label;
            }

            if (valueText != null)
            {
                valueText.text = value;
            }
        }

        private void ResolveReferences()
        {
            if (labelText == null)
            {
                labelText = transform.Find("Content/Label")?.GetComponent<TMP_Text>()
                    ?? transform.Find("Label")?.GetComponent<TMP_Text>();
            }

            if (valueText == null)
            {
                valueText = transform.Find("Content/Value")?.GetComponent<TMP_Text>()
                    ?? transform.Find("Value")?.GetComponent<TMP_Text>();
            }
        }
    }
}
