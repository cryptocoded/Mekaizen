using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mechs
{
    public enum StatType
    {
        Armor,
        PowerGeneration,
        PowerConsumption,
        Speed,
        Damage,
        Range,
        Accuracy,
        Weight
    }

    public enum ComponentType
    {
        Weapon,
        Armor,
        Mobility,
        Reactor,
        Utility,
        Sensor,
        System
    }

    public enum EffectKind
    {
        Passive,        // always-on modifiers
        ActiveAbility,  // player/AI triggered during combat
        SetBonus        // activates when set requirements are met
    }

    [Serializable]
    public struct StatEntry
    {
        public StatType Type;
        public float Value;
    }

    /// <summary>
    /// Additive and multiplicative modifiers. Final aggregation uses:
    /// final = (base + sum(Add)) * product(Mult)
    /// </summary>
    [Serializable]
    public struct StatModifier
    {
        public StatType Type;
        public float Add;   // e.g., +10 armor
        public float Mult;  // e.g., 1.1f for +10% (use 1f for none)

        public static StatModifier Additive(StatType t, float add) => new StatModifier { Type = t, Add = add, Mult = 1f };
        public static StatModifier Multiplicative(StatType t, float mult) => new StatModifier { Type = t, Add = 0f, Mult = mult };
    }

    /// <summary>
    /// Simple, serializable sheet you can inspect in the debugger.
    /// </summary>
    [Serializable]
    public class StatSheet
    {
        [SerializeField] private Dictionary<StatType, float> _values = new Dictionary<StatType, float>();

        public float Get(StatType t) => _values.TryGetValue(t, out var v) ? v : 0f;
        public void Set(StatType t, float v) => _values[t] = v;
        public void Add(StatType t, float v) => Set(t, Get(t) + v);

        public IReadOnlyDictionary<StatType, float> AsReadOnly() => _values;
        public StatSheet Clone()
        {
            var clone = new StatSheet();
            foreach (var kv in _values) clone._values[kv.Key] = kv.Value;
            return clone;
        }
    }

    public enum SlotType
    {
        None = 0,
        Legs,
        Waist,
        Core,
        Chest,
        Arms,
        Head,
        Back
    }

}
