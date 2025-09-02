using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mechs
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Mechs/Mech Runtime")]
    public class MechRuntime : MonoBehaviour
    {
        // ---------------- Public API ----------------
        public Action StatsChanged;

        [SerializeField] private StatSheet finalStats = new StatSheet();
        public StatSheet FinalStats => finalStats;

        public float NetPower =>
            FinalStats.Get(StatType.PowerGeneration) - FinalStats.Get(StatType.PowerConsumption);

        private List<IActiveAbility> _activeAbilities = new();
        public IReadOnlyList<IActiveAbility> ActiveAbilities => _activeAbilities;

        // Optional quick defaults for testing in-editor
        [Serializable] private struct SlotDefault { public SlotType Slot; public MechComponentSO Component; }
        [Header("Optional Defaults (debug)")]
        [SerializeField] private List<SlotDefault> defaultLoadout = new();
        [SerializeField] private bool applyDefaultsOnEnable = true;

        // ---------------- Internal state ----------------
        // Structural components: exactly one per body slot.
        private readonly Dictionary<SlotType, MechComponentSO> structural =
            new Dictionary<SlotType, MechComponentSO>
            {
                { SlotType.Legs,  null },
                { SlotType.Waist, null },
                { SlotType.Core,  null },
                { SlotType.Chest, null },
                { SlotType.Arms,  null },
                { SlotType.Head,  null },
                { SlotType.Back,  null },
            };

        // Per-slot hardpoint managers (capacity mirrors equipped structural.HardpointCount)
        private readonly Dictionary<SlotType, HardpointsManager> hardpoints =
            new Dictionary<SlotType, HardpointsManager>();

        private List<SpecialEffectSO> _equippedEffects = new();

        // ---------------- Unity ----------------
        private void OnEnable()
        {
            if (applyDefaultsOnEnable && defaultLoadout != null)
            {
                foreach (var d in defaultLoadout)
                {
                    if (d.Component != null) EquipStructural(d.Component);
                }
            }
            RebuildAll();
        }

        // ---------------- Structural API ----------------
        public MechComponentSO GetStructural(SlotType slot) =>
            structural.TryGetValue(slot, out var c) ? c : null;

        /// <summary>
        /// Equip a structural part into its declared slot.
        /// Rebuilds that slot's HardpointsManager to the part's HardpointCount.
        /// </summary>
        public bool EquipStructural(MechComponentSO component)
        {
            if (component == null || !component.IsStructural)
            {
                Debug.LogWarning("EquipStructural failed: null or non-structural.");
                return false;
            }

            var slot = component.Slot;
            if (!structural.ContainsKey(slot))
            {
                Debug.LogWarning($"EquipStructural failed: invalid slot {slot}.");
                return false;
            }

            structural[slot] = component;

            int cap = Mathf.Max(0, component.HardpointCount);
            if (cap > 0)
            {
                if (!hardpoints.TryGetValue(slot, out var mgr))
                    hardpoints[slot] = new HardpointsManager(cap);
                else
                    mgr.ResetCapacity(cap); // NOTE: clears mounts. Change if you want to preserve where possible.
            }
            else
            {
                hardpoints.Remove(slot);
            }

            RebuildAll();
            return true;
        }

        public void UnequipStructural(SlotType slot)
        {
            if (!structural.ContainsKey(slot)) return;
            structural[slot] = null;
            hardpoints.Remove(slot);
            RebuildAll();
        }

        // ---------------- Hardpoints / Weapons (multi-HP contiguous) ----------------
        public HardpointsManager GetHardpointsManager(SlotType slot) =>
            hardpoints.TryGetValue(slot, out var mgr) ? mgr : null;

        public int GetHardpointCount(SlotType slot) =>
            GetHardpointsManager(slot)?.TotalPoints ?? 0;

        public MechComponentSO GetMountedWeapon(SlotType slot, int hardpointIndex) =>
            GetHardpointsManager(slot)?.GetAt(hardpointIndex);

        /// <summary>
        /// Mount a weapon starting at startIndex. Requires weapon.HardpointsRequired contiguous points
        /// within the same slot's capacity.
        /// </summary>
        public bool MountWeapon(SlotType slot, int startIndex, MechComponentSO weapon)
        {
            if (weapon == null || !weapon.IsWeapon) return false;
            var mgr = GetHardpointsManager(slot);
            if (mgr == null) return false;

            bool ok = mgr.TryMountAt(weapon, startIndex);
            if (ok) RebuildAll();
            return ok;
        }

        /// <summary>
        /// Unmount the entire weapon occupying the given index on the slot.
        /// </summary>
        public void UnmountWeapon(SlotType slot, int anyIndexOfWeapon)
        {
            var mgr = GetHardpointsManager(slot);
            if (mgr == null) return;
            mgr.UnmountByIndex(anyIndexOfWeapon);
            RebuildAll();
        }

        // ---------------- Aggregation ----------------
        public void RebuildAll()
        {
            var baseTotals = new Dictionary<StatType, float>();
            _equippedEffects = new List<SpecialEffectSO>();
            _activeAbilities = new List<IActiveAbility>();

            // 1) Structural parts
            IEnumerable<MechComponentSO> comps = structural.Values.Where(c => c != null);

            // 2) Distinct mounted weapons per slot (avoid double-counting multi-HP spans)
            var weaponSet = new HashSet<MechComponentSO>();
            foreach (var kv in hardpoints)
                foreach (var w in kv.Value.DistinctMounted())
                    weaponSet.Add(w);

            comps = comps.Concat(weaponSet);

            // 3) Sum base stats & collect effects/abilities
            foreach (var c in comps)
            {
                foreach (var sb in c.BaseStats)
                    sb.AddTo(baseTotals);


                if (c.Effects != null)
                {
                    foreach (var eff in c.Effects)
                    {
                        if (eff == null) continue;
                        _equippedEffects.Add(eff);
                        eff.OnEquip(this);
                        if (eff is IActiveAbility aa) _activeAbilities.Add(aa);
                    }
                }
            }

            // 4) Build modifiers (passives + set bonuses)
            var modifiers = new List<StatModifier>();
            foreach (var eff in _equippedEffects)
            {
                if (eff is ISetBonusProvider setBonus)
                {
                    if (setBonus.IsActive(this))
                        modifiers.AddRange(setBonus.GetModifiers(this));
                }
                else if (eff is IProvidesStatModifiers psm)
                {
                    modifiers.AddRange(psm.GetModifiers(this));
                }
            }

            // 5) Final = (base + sum(Add)) * product(Mult)
            finalStats = new StatSheet();
            foreach (var kv in baseTotals)
                finalStats.Set(kv.Key, kv.Value);

            foreach (var group in modifiers.GroupBy(m => m.Type))
            {
                float add = group.Sum(m => m.Add);
                float mult = 1f;
                foreach (var m in group) mult *= (m.Mult == 0f ? 1f : m.Mult);
                finalStats.Set(group.Key, (finalStats.Get(group.Key) + add) * mult);
            }

            if (NetPower < 0f)
                Debug.LogWarning($"{name} has negative NetPower ({NetPower}). Check reactor vs consumption.");

            StatsChanged?.Invoke();
        }

        // ---------------- Helpers for UI / sets ----------------
        /// <summary> All equipped: structural + distinct weapons. </summary>
        public IEnumerable<MechComponentSO> AllEquipped()
        {
            foreach (var s in structural.Values) if (s != null) yield return s;

            var seen = new HashSet<MechComponentSO>();
            foreach (var kv in hardpoints)
            {
                foreach (var w in kv.Value.DistinctMounted())
                {
                    if (seen.Add(w)) yield return w;
                }
            }
        }

        public int CountByTag(string tag) => AllEquipped().Count(c => c.HasTag(tag));

        public SlotType GetParentSlotOfWeapon(MechComponentSO weapon)
        {
            foreach (var kv in hardpoints)
            {
                if (kv.Value.DistinctMounted().Contains(weapon))
                    return kv.Key;
            }
            return SlotType.None;
        }
    }
}
