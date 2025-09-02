using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mechs.UI
{
    public class HardpointButtonUI : MonoBehaviour
    {
        public Button Button;
        public Image Icon;
        public TMP_Text Label;       // display name
        public TMP_Text IndexText;   // optional - for sorting

        public void Bind(System.Action onClick) {
            if (Button == null) Button = GetComponent<Button>();
            if (Button != null) {
                Button.onClick.RemoveAllListeners();
                Button.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        public void Set(MechComponentSO weapon, int displayIndex) {
            if (IndexText) IndexText.text = displayIndex.ToString();
            if (Label)     Label.text = weapon ? weapon.DisplayName : "Weapon";
            if (Icon) {
                Icon.sprite  = weapon ? weapon.Icon : null;
                Icon.enabled = weapon && weapon.Icon != null;
            }
        }
    }
}
