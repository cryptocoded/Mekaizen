using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mechs
{
    [System.Serializable]
    public class HardpointsManager
    {
        [SerializeField] private int _capacity;
        [SerializeField] private MechComponentSO[] _occ; // occupancy: one entry per hardpoint index

        public int TotalPoints => _capacity;

        public HardpointsManager(int capacity = 0)
        {
            ResetCapacity(capacity);
        }

        public void ResetCapacity(int capacity)
        {
            _capacity = Mathf.Max(0, capacity);
            _occ = new MechComponentSO[_capacity];
        }

        public int Count => _capacity;

        public MechComponentSO GetAt(int index)
        {
            if (index < 0 || index >= _capacity) return null;
            return _occ[index];
        }

        public IEnumerable<MechComponentSO> DistinctMounted()
        {
            var set = new HashSet<MechComponentSO>();
            for (int i = 0; i < _capacity; i++)
                if (_occ[i] != null) set.Add(_occ[i]);
            return set;
        }

        public bool IsRangeFree(int start, int length)
        {
            if (length <= 0) return false;
            if (start < 0 || start + length > _capacity) return false;
            for (int i = 0; i < length; i++)
                if (_occ[start + i] != null) return false;
            return true;
        }

        public bool CanMountAt(MechComponentSO weapon, int startIndex)
        {
            if (weapon == null || !weapon.IsWeapon) return false;
            int need = Mathf.Max(1, weapon.HardpointsRequired);
            return IsRangeFree(startIndex, need);
        }

        /// <summary>
        /// Mounts a weapon starting at index; consumes weapon.HardpointsRequired contiguous points.
        /// Returns true on success.
        /// </summary>
        public bool TryMountAt(MechComponentSO weapon, int startIndex)
        {
            if (!CanMountAt(weapon, startIndex)) return false;
            int need = Mathf.Max(1, weapon.HardpointsRequired);
            for (int i = 0; i < need; i++)
                _occ[startIndex + i] = weapon;
            return true;
        }

        /// <summary>
        /// Unmounts the entire weapon occupying the hardpoint at 'index'.
        /// </summary>
        public void UnmountByIndex(int index)
        {
            var w = GetAt(index);
            if (w == null) return;
            for (int i = 0; i < _capacity; i++)
                if (_occ[i] == w) _occ[i] = null;
        }

        /// <summary>
        /// Remove all mounted weapons (keeps capacity intact).
        /// </summary>
        public void ClearAllMounts()
        {
            for (int i = 0; i < _capacity; i++) _occ[i] = null;
        }

        /// <summary>
        /// Optional helper: indices where a given weapon *could* start right now.
        /// </summary>
        public IEnumerable<int> ValidStartIndicesFor(MechComponentSO weapon)
        {
            if (weapon == null || !weapon.IsWeapon) yield break;
            int need = Mathf.Max(1, weapon.HardpointsRequired);
            for (int s = 0; s + need <= _capacity; s++)
                if (IsRangeFree(s, need)) yield return s;
        }
    }
}
