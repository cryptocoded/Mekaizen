using UnityEngine;

namespace Mechs
{
    [CreateAssetMenu(menuName = "Mechs/Effects/Active Ability")]
    public class ActiveAbilitySO : SpecialEffectSO, IActiveAbility
    {
        [SerializeField] private string abilityName = "Ability";
        [SerializeField] private float cooldownSeconds = 10f;
        [SerializeField] private float energyCost = 10f;

        public string AbilityName => abilityName;
        public float CooldownSeconds => cooldownSeconds;
        public float EnergyCost => energyCost;

        // Stub; wire your combat system here.
        public virtual void Execute(MechRuntime user, MechRuntime target)
        {
            Debug.Log($"Executing {abilityName} from {user.name} on {target?.name ?? "no target"}.");
            // Example: apply damage based on user's Damage stat:
            // float dmg = user.FinalStats.Get(StatType.Damage);
            // target.ApplyDamage(dmg);
        }
    }
}
