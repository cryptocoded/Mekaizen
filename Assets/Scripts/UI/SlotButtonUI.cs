using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mechs.UI
{
    public class SlotButtonUI : MonoBehaviour
    {
        public SlotType Slot;

        [Header("Structural")]
        public Button StructuralButton;
        public Image StructuralIcon;
        public TMP_Text StructuralLabel;

        [Header("Hardpoints")]
        [Tooltip("Template button for a single hardpoint; will be cloned per HP. Keep inactive in prefab.")]
        public Button WeaponButtonTemplate;

        [Tooltip("Parent for the cloned hardpoint buttons (put a Horizontal/Vertical Layout Group here for spacing).")]
        public RectTransform HardpointButtonsParent;

        [Header("Per-index visuals (optional)")]
        public Image WeaponIcon;   // legacy single icon – we’ll set based on the first mounted weapon if present
        public TMP_Text WeaponLabel;

        [SerializeField] private List<Button> ActiveHardpointButtons = new();

        private LoadoutUIController controller;

        public void Bind(LoadoutUIController ctl) => controller = ctl;

        private void Awake()
        {
            if (StructuralButton) StructuralButton.onClick.AddListener(() => controller?.SelectStructural(Slot));
            if (WeaponButtonTemplate) WeaponButtonTemplate.gameObject.SetActive(false); // hide template
        }

        public void Refresh(Mechs.MechRuntime mech)
        {
            var structural = mech.GetStructural(Slot);
            var hpMgr = mech.GetHardpointsManager(Slot);

            // Structural visuals
            if (StructuralLabel) StructuralLabel.text = structural ? structural.DisplayName : $"[{Slot}]";
            if (StructuralIcon)
            {
                StructuralIcon.sprite  = structural ? structural.Icon : null;
                StructuralIcon.enabled = structural && structural.Icon != null;
            }

            // Destroy previous HP buttons
            for (int i = 0; i < ActiveHardpointButtons.Count; i++)
                if (ActiveHardpointButtons[i]) Destroy(ActiveHardpointButtons[i].gameObject);
            ActiveHardpointButtons.Clear();

            int hpCount = hpMgr?.TotalPoints ?? 0;
            if (HardpointButtonsParent) HardpointButtonsParent.gameObject.SetActive(hpCount > 0);
            if (hpCount <= 0 || WeaponButtonTemplate == null || HardpointButtonsParent == null)
            {
                if (WeaponLabel) WeaponLabel.text = hpCount > 0 ? "Weapon" : "";
                if (WeaponIcon)  WeaponIcon.enabled = false;
                return;
            }

            // Spawn one button per HP index
            for (int i = 0; i < hpCount; i++)
            {
                var btn = Instantiate(WeaponButtonTemplate, HardpointButtonsParent);
                btn.gameObject.SetActive(true);

                int hpIndex = i; // capture for lambda
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => controller?.SelectWeaponForSlot(Slot, hpIndex));

                // Per-index visuals (optional)
                var w = hpMgr.GetAt(i); // might be null or part of a multi-HP weapon
                var label = btn.GetComponentInChildren<TMP_Text>(true);
                var icon  = btn.GetComponentInChildren<Image>(true);

                if (label) label.text = w ? w.DisplayName : (i + 1).ToString(); // show index if empty
                if (icon)
                {
                    // If your button has multiple Images, consider finding a specific child by name/tag
                    icon.sprite  = w ? w.Icon : null;
                    icon.enabled = w && w.Icon != null;
                }

                ActiveHardpointButtons.Add(btn);
            }

            // Legacy single icon/label = first mounted weapon (if any)
            var firstMounted = (hpMgr != null) ? FirstMounted(hpMgr) : null;
            if (WeaponLabel) WeaponLabel.text = firstMounted ? firstMounted.DisplayName : "Weapon";
            if (WeaponIcon)
            {
                WeaponIcon.sprite  = firstMounted ? firstMounted.Icon : null;
                WeaponIcon.enabled = firstMounted && firstMounted.Icon != null;
            }
        }

        private static MechComponentSO FirstMounted(HardpointsManager mgr)
        {
            foreach (var w in mgr.DistinctMounted()) return w;
            return null;
        }
    }
}
