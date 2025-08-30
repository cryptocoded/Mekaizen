using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mechs
{
    public class MechRuntime : MonoBehaviour
    {
        [Header("Equipped Structural Components (exactly one each)")]
        [SerializeField] private Dictionary<SlotType, MechComponentSO> structural =
            new Dictionary<SlotType, MechComponentSO>
            {
                { SlotType.Legs, null },
                { SlotType.Waist, null },
                { SlotType.Core, null },
                { SlotType.Chest, null },
                { SlotType.Arms, null },
                { SlotType.Head, null },
                { SlotType.Back, null },
            };

        [Header("Per-Slot Hardpoints (auto-created if the installed structural provides one)")]
        [SerializeField] private Dictionary<SlotType, HardpointsManager> hardpoints =
            new Dictionary<SlotType, HardpointsManager>();

        [Header("Computed (read-only in inspector)")]
        [SerializeField] private StatSheet finalStats = new StatSheet();
        public StatSheet FinalStats => finalStats;
        public System.Action StatsChanged;

        private readonly List<IActiveAbility> _activeAbilities = new List<IActiveAbility>();
        public IReadOnlyList<IActiveAbility> ActiveAbilities => _activeAbilities;

        private readonly List<SpecialEffectSO> _equippedEffects = new List<SpecialEffectSO>();
        public float NetPower => FinalStats.Get(StatType.PowerGeneration) - FinalStats.Get(StatType.PowerConsumption);

        private void OnEnable() => RebuildAll();

        // ---------------- EQUIP API ----------------

        /// <summary>Equip a structural component into its declared Slot. Enforces one-per-slot.</summary>
        public bool EquipStructural(MechComponentSO component)
        {
            if (component == null || !component.IsStructural)
            {
                Debug.LogWarning("EquipStructural failed: component is null or not structural.");
                return false;
            }

            var s = component.Slot;
            if (!structural.ContainsKey(s))
            {
                Debug.LogWarning($"EquipStructural failed: invalid slot {s}.");
                return false;
            }

            structural[s] = component;

            // Manage hardpoint for this slot
            var numHardpoints = component.HardpointsManager.TotalPoints;
            if (numHardpoints > 0)
            {
                if (!hardpoints.ContainsKey(s))
                {
                    hardpoints[s] = new HardpointsManager(s);
                }
            }
            else
            {
                // Remove hardpoint & any mounted weapon if the new structural doesn't provide it
                if (hardpoints.ContainsKey(s))
                    hardpoints.Remove(s);
            }

            RebuildAll();
            return true;
        }

        /// <summary>Unequip a structural component from the given slot (and its weapon if any).</summary>
        public void UnequipStructural(SlotType slot)
        {
            if (!structural.ContainsKey(slot)) return;
            structural[slot] = null;
            if (hardpoints.ContainsKey(slot))
                hardpoints.Remove(slot);
            RebuildAll();
        }

        /// <summary>Mount a Weapon into the hardpoint of the given slot.</summary>
        public bool MountWeapon(SlotType slot, MechComponentSO weapon)
        {
            if (weapon == null || !weapon.IsWeapon)
            {
                Debug.LogWarning("MountWeapon failed: not a weapon.");
                return false;
            }

            if (!hardpoints.TryGetValue(slot, out var hp))
            {
                Debug.LogWarning($"MountWeapon failed: slot {slot} has no hardpoint.");
                return false;
            }

            var ok = hp.Mount(weapon);
            if (ok) RebuildAll();
            return ok;
        }

        public void UnmountWeapon(SlotType slot, MechComponentSO weapon)
        {
            if (hardpoints.TryGetValue(slot, out var points))
            {
                points.Unmount(weapon);
                RebuildAll();
            }
        }

        // --------------- BUILD / AGGREGATION ----------------

        public void RebuildAll()
        {
            _equippedEffects.Clear();
            _activeAbilities.Clear();

            // Gather components: all installed structural + any mounted weapons
            IEnumerable<MechComponentSO> allComps = structural.Values.Where(c => c != null);
            var weaponComps = hardpoints.Values
                                        .Select(hp => hp.MountedWeapons)
                                        .Where(w => w != null)
                                        .SelectMany(x => x)
                                        .ToList();;

            allComps = allComps.Concat(weaponComps);

            // Base stats
            var baseStats = new Dictionary<StatType, float>();
            foreach (var comp in allComps)
            {
                foreach (var s in comp.BaseStats)
                {
                    if (!baseStats.ContainsKey(s.Type)) baseStats[s.Type] = 0f;
                    baseStats[s.Type] += s.Value;
                }

                foreach (var eff in comp.Effects)
                {
                    if (eff == null) continue;
                    _equippedEffects.Add(eff);
                    eff.OnEquip(this);

                    if (eff is IActiveAbility aa)
                        _activeAbilities.Add(aa);
                }
            }

            // Modifiers (passives + active set bonuses)
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

            // Final = (base + sum(Add)) * product(Mult)
            finalStats = new StatSheet();
            foreach (var kv in baseStats) finalStats.Set(kv.Key, kv.Value);

            var byStat = modifiers.GroupBy(m => m.Type);
            foreach (var group in byStat)
            {
                float baseVal = finalStats.Get(group.Key);
                float add = group.Sum(m => m.Add);
                float mult = 1f;
                foreach (var m in group) mult *= (m.Mult == 0f ? 1f : m.Mult);
                finalStats.Set(group.Key, (baseVal + add) * mult);
            }

            // Simple sanity check
            if (NetPower < 0f)
                Debug.LogWarning($"{name} has negative NetPower ({NetPower}). Check reactor vs consumption.");

            StatsChanged?.Invoke();

        }

        // --------------- Query helpers ----------------

        public MechComponentSO GetStructural(SlotType slot) =>
            structural.TryGetValue(slot, out var c) ? c : null;

        public HardpointsManager GetHardpoint(SlotType slot) =>
            hardpoints.TryGetValue(slot, out var hp) ? hp : null;

        public IEnumerable<MechComponentSO> AllEquipped()
        {
            foreach (var s in structural.Values) if (s != null) yield return s;
            //foreach (var w in hardpoints.Values) if (w.MountedWeapon != null) yield return w.MountedWeapon; - I don't think we care if all hardpoints are filled
        }

        public SlotType GetParentSlotOfWeapon(MechComponentSO weapon)
        {
            foreach (var kv in hardpoints)
                if (kv.Value.MountedWeapons.Contains(weapon))
                    return kv.Key;
            return SlotType.None;
        }

        // Count of components with a given tag (used by set bonuses)
        public int CountByTag(string tag) => AllEquipped().Count(c => c.HasTag(tag));
    }
}
