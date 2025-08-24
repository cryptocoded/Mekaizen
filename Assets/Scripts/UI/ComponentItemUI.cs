using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mechs.UI
{
    public class ComponentItemUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text subText; // Type / Slot / Tags
        [SerializeField] private Button button;

        private MechComponentSO data;
        private LoadoutUIController controller;

        public void Init(MechComponentSO comp, LoadoutUIController ctl)
        {
            data = comp;
            controller = ctl;

            if (nameText) nameText.text = comp.DisplayName;
            if (icon)
            {
                icon.sprite = comp.Icon;
                icon.enabled = comp.Icon != null;
            }
            if (subText)
            {
                string slotTxt = comp.IsStructural ? comp.Slot.ToString() : "Weapon";
                string typeTxt = comp.Type.ToString();
                subText.text = $"{typeTxt} • {slotTxt}";
            }

            if (button)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => controller?.OnInventoryItemClicked(data));
            }
        }
    }
}
