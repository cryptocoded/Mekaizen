using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mechs.UI
{
    public class SlotButtonUI : MonoBehaviour
    {
        public SlotType Slot;
        public Button StructuralButton;
        public Button WeaponButton; // small overlay button
        public Image StructuralIcon;
        public Image WeaponIcon;
        public TMP_Text StructuralLabel;
        public TMP_Text WeaponLabel;

        private LoadoutUIController controller;

        public void Bind(LoadoutUIController ctl) => controller = ctl;

        private void Awake()
        {
            if (StructuralButton) StructuralButton.onClick.AddListener(() => controller?.SelectStructural(Slot));
            if (WeaponButton)     WeaponButton.onClick.AddListener(() => controller?.SelectWeaponForSlot(Slot));
        }

        public void Refresh(Mechs.MechRuntime mech)
        {
            var structural = mech.GetStructural(Slot);
            var hp = mech.GetHardpoint(Slot);
            // Structural visuals
            if (StructuralLabel) StructuralLabel.text = structural ? structural.DisplayName : $"[{Slot}]";
            if (StructuralIcon)  StructuralIcon.sprite = structural ? structural.Icon : null;
            if (StructuralIcon)  StructuralIcon.enabled = structural && structural.Icon != null;

            // Weapon visuals / availability
            bool hasHP = hp != null;
            if (WeaponButton) WeaponButton.gameObject.SetActive(hasHP);

            var weapon = hasHP ? hp.MountedWeapon : null;
            if (WeaponLabel) WeaponLabel.text = weapon ? weapon.DisplayName : (hasHP ? "Weapon" : "");
            if (WeaponIcon)
            {
                WeaponIcon.sprite = weapon ? weapon.Icon : null;
                WeaponIcon.enabled = weapon && weapon.Icon != null;
            }
        }
    }
}
