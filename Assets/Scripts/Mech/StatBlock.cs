// StatBlock.cs (or add inside StatPrimitives.cs)
// Purpose: fixed set of base stats with 0 defaults, plus helpers for aggregation.

using System;
using System.Collections.Generic;

namespace Mechs
{
    [Serializable]
    public struct StatBlock
    {
        public float Armor;
        public float PowerGeneration;
        public float PowerConsumption;
        public float Speed;
        public float Damage;
        public float Range;
        public float Accuracy;
        public float Weight;

        // Iterate as StatEntry records (handy for debugging/tools)
        public IEnumerable<StatEntry> Enumerate()
        {
            yield return new StatEntry { Type = StatType.Armor,            Value = Armor };
            yield return new StatEntry { Type = StatType.PowerGeneration,  Value = PowerGeneration };
            yield return new StatEntry { Type = StatType.PowerConsumption, Value = PowerConsumption };
            yield return new StatEntry { Type = StatType.Speed,            Value = Speed };
            yield return new StatEntry { Type = StatType.Damage,           Value = Damage };
            yield return new StatEntry { Type = StatType.Range,            Value = Range };
            yield return new StatEntry { Type = StatType.Accuracy,         Value = Accuracy };
            yield return new StatEntry { Type = StatType.Weight,           Value = Weight };
        }

        // Add this block's values into a totals dictionary
        public void AddTo(Dictionary<StatType, float> totals)
        {
            if (totals == null) return;
            void add(StatType t, float v)
            {
                if (v == 0f) return;
                if (totals.TryGetValue(t, out var cur)) totals[t] = cur + v;
                else totals[t] = v;
            }

            add(StatType.Armor,            Armor);
            add(StatType.PowerGeneration,  PowerGeneration);
            add(StatType.PowerConsumption, PowerConsumption);
            add(StatType.Speed,            Speed);
            add(StatType.Damage,           Damage);
            add(StatType.Range,            Range);
            add(StatType.Accuracy,         Accuracy);
            add(StatType.Weight,           Weight);
        }
    }
}
