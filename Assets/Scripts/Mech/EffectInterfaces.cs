using System.Collections.Generic;

namespace Mechs
{
    public interface IProvidesStatModifiers
    {
        IEnumerable<StatModifier> GetModifiers(MechRuntime mech);
    }

    public interface IActiveAbility
    {
        string AbilityName { get; }
        float CooldownSeconds { get; }
        float EnergyCost { get; }

        // Minimal signature; expand with context, targets, hit rolls, etc.
        void Execute(MechRuntime user, MechRuntime target);
    }

    public interface ISetBonusProvider : IProvidesStatModifiers
    {
        string SetTag { get; }
        int RequiredCount { get; }
        bool IsActive(MechRuntime mech); // given current loadout
    }
}
