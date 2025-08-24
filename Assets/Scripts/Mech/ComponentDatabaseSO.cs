using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mechs
{
    [CreateAssetMenu(menuName = "Mechs/Database")]
    public class ComponentDatabaseSO : ScriptableObject
    {
        public List<MechComponentSO> AllComponents = new();

        public IEnumerable<MechComponentSO> GetStructuralForSlot(SlotType slot) =>
            AllComponents.Where(c => c != null && c.IsStructural && c.Slot == slot);

        public IEnumerable<MechComponentSO> GetWeapons() =>
            AllComponents.Where(c => c != null && c.IsWeapon);
    }
}
