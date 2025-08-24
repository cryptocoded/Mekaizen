using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mechs
{
    [CreateAssetMenu(menuName = "Mechs/Effects/Set Bonus")]
    public class SetBonusSO : SpecialEffectSO, ISetBonusProvider
    {
        [SerializeField] private string setTag;
        [SerializeField] private int requiredCount = 2;
        [SerializeField] private List<StatModifier> modifiers = new List<StatModifier>();

        public string SetTag => setTag;
        public int RequiredCount => requiredCount;

        public bool IsActive(MechRuntime mech)
        {
            if (mech == null) return false;
            int count = mech.CountByTag(setTag);
            return count >= requiredCount;
        }

        public IEnumerable<StatModifier> GetModifiers(MechRuntime mech)
        {
            return IsActive(mech) ? modifiers : System.Array.Empty<StatModifier>();
        }
    }
}
