using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

namespace Mechs
{
    [System.Serializable]
    public class HardpointsManager
    {
        [SerializeField] private SlotType parentSlot;
        [SerializeField] private List<MechComponentSO> mountedWeapons = new(); // must be Type=Weapon
        public int TotalPoints = 1;
        [SerializeField] private int currentPoints = 1;

        public SlotType ParentSlot => parentSlot;
        public List<MechComponentSO> MountedWeapons => mountedWeapons;

        public HardpointsManager(SlotType parent) { parentSlot = parent; }

        public bool CanMount(MechComponentSO comp)
        {
            if (comp == null)
                return false;
            if (!comp.IsWeapon)
                return false;
            if (comp.RequiredHardpoints > currentPoints)
                    return false;

            currentPoints -= comp.RequiredHardpoints;
            return true;
        }

        public bool Mount(MechComponentSO weapon)
        {
            if (!CanMount(weapon)) return false;
            mountedWeapons.Add(weapon);
            return true;
        }

        public void Unmount(MechComponentSO weapon)
        {
            mountedWeapons.Remove(weapon);
        }
    }
}
