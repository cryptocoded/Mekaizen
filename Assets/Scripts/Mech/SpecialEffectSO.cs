using UnityEngine;

namespace Mechs
{
    public abstract class SpecialEffectSO : ScriptableObject
    {
        [SerializeField] private EffectKind kind;
        [TextArea] public string Description;

        public EffectKind Kind => kind;

        // Optional: hooks if you want to listen to equip/unequip or combat events later.
        public virtual void OnEquip(MechRuntime mech) { }
        public virtual void OnUnequip(MechRuntime mech) { }
    }
}
