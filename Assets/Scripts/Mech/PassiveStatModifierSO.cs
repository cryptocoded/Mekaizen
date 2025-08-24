using System.Collections.Generic;
using UnityEngine;

namespace Mechs
{
    [CreateAssetMenu(menuName = "Mechs/Effects/Passive Stat Modifier")]
    public class PassiveStatModifierSO : SpecialEffectSO, IProvidesStatModifiers
    {
        [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();

        public IEnumerable<StatModifier> GetModifiers(MechRuntime mech)
        {
            return modifiers;
        }
    }
}
