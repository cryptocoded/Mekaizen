using UnityEngine;

namespace Mechs
{
    [System.Serializable]
    public class Hardpoint
    {
        [SerializeField] private SlotType parentSlot;
        [SerializeField] private MechComponentSO mountedWeapon; // must be Type=Weapon

        public SlotType ParentSlot => parentSlot;
        public MechComponentSO MountedWeapon => mountedWeapon;

        public Hardpoint(SlotType parent) { parentSlot = parent; }

        public bool CanMount(MechComponentSO comp)
        {
            return comp != null && comp.IsWeapon;
        }

        public bool Mount(MechComponentSO weapon)
        {
            if (!CanMount(weapon)) return false;
            mountedWeapon = weapon;
            return true;
        }

        public void Unmount()
        {
            mountedWeapon = null;
        }
    }
}
