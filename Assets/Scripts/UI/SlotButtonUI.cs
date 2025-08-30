using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Mechs.UI
{
    public class SlotButtonUI : MonoBehaviour
    {
        public SlotType Slot;
        public Button StructuralButton;
        public Button WeaponButton; // small overlay button - should probably be a prefab.
        public List<Button> ActiveHardpointButtons;
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
            var hardpoints = structural != null ? structural.HardpointsManager : null;// mech.GetHardpoint(Slot);
            // Structural visuals
            if (StructuralLabel) StructuralLabel.text = structural ? structural.DisplayName : $"[{Slot}]";
            if (StructuralIcon)  StructuralIcon.sprite = structural ? structural.Icon : null;
            if (StructuralIcon)  StructuralIcon.enabled = structural && structural.Icon != null;

            // Weapon visuals / availability
            
            bool hasHardpoint = hardpoints?.TotalPoints > 0;
            

            if (WeaponButton) WeaponButton.gameObject.SetActive(hasHardpoint);
            if (!hasHardpoint)
                return;
            for (int i = 0; i < hardpoints.TotalPoints; i++)
            {
                var newButton = Instantiate(WeaponButton);
                newButton.transform.SetParent(WeaponButton.transform.parent);
                newButton.transform.position = new Vector3(WeaponButton.transform.position.x - 30*i, WeaponButton.transform.position.y, WeaponButton.transform.position.z);
                ActiveHardpointButtons.Add(newButton);
            }

            //This will only display icons for the first mounted weapon. Change this to support many.
                var weapon = hasHardpoint ? hardpoints.MountedWeapons?.FirstOrDefault() : null;
            if (WeaponLabel) WeaponLabel.text = weapon ? weapon.DisplayName : (hasHardpoint ? "Weapon" : "");
            if (WeaponIcon)
            {
                WeaponIcon.sprite = weapon ? weapon.Icon : null;
                WeaponIcon.enabled = weapon && weapon.Icon != null;
            }
        }
    }
}
