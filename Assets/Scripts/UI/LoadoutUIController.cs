using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mechs;

namespace Mechs.UI
{
    public class LoadoutUIController : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private Mechs.MechRuntime mech;
        [SerializeField] private ComponentDatabaseSO database;

        [Header("Paper Doll Slots")]
        [SerializeField] private SlotButtonUI legs;
        [SerializeField] private SlotButtonUI waist;
        [SerializeField] private SlotButtonUI core;
        [SerializeField] private SlotButtonUI chest;
        [SerializeField] private SlotButtonUI arms;
        [SerializeField] private SlotButtonUI head;
        [SerializeField] private SlotButtonUI back;

        [Header("Inventory UI")]
        [SerializeField] private Transform inventoryGrid; // Content transform of a ScrollView
        [SerializeField] private ComponentItemUI itemPrefab;
        [SerializeField] private TMP_InputField searchField;
        [SerializeField] private TMP_Text headerText;

        private readonly List<ComponentItemUI> _spawned = new();

        private enum SelectMode { None, Structural, WeaponForHardpoint }
        private SelectMode mode = SelectMode.None;

        private int selectedHardpointIndex = 0;
        private SlotType selectedSlot = SlotType.None;

        private void Awake()
        {
            // Bind slot buttons to this controller
            foreach (var s in AllSlotUIs())
                s.Bind(this);

            if (searchField) searchField.onValueChanged.AddListener(_ => PopulateInventory());
            if (mech != null) mech.StatsChanged += RefreshPaperDoll;
        }

        private void OnEnable()
        {
            // Default to structural selection for Arms to get started
            SelectStructural(SlotType.Head);
            RefreshPaperDoll();
        }

        private IEnumerable<SlotButtonUI> AllSlotUIs()
        {
            yield return legs; yield return waist; yield return core; yield return chest;
            yield return arms; yield return head; yield return back;
        }

        public void SelectStructural(SlotType slot)
        {
            mode = SelectMode.Structural;
            selectedSlot = slot;
            if (headerText) headerText.text = $"Select Component for {slot}";
            PopulateInventory();
            HighlightSelectedSlot(slot);
        }

        public void SelectWeaponForSlot(SlotType slot, int hardpointIndex)
        {
            // Guard: ensure the slot actually has that hardpoint
            int count = mech.GetHardpointCount(slot);
            if (hardpointIndex < 0 || hardpointIndex >= count)
            {
                Debug.LogWarning($"{slot} hardpoint {hardpointIndex} is out of range (0..{count-1}).");
                return;
            }

            mode = SelectMode.WeaponForHardpoint;
            selectedSlot = slot;
            selectedHardpointIndex = hardpointIndex;

            if (headerText) headerText.text = $"Select Weapon for {slot} HP {hardpointIndex+1}";
            PopulateInventory();     // filter to weapons as you already do
            HighlightSelectedSlot(slot);
        }


        private void HighlightSelectedSlot(SlotType slot)
        {
            // (Optional) Add a visual highlight—e.g., scale, outline, color—if you want.
            // Here we just refresh paper doll so labels/icons show current equip.
            RefreshPaperDoll();
        }

        private void ClearInventory()
        {
            foreach (var it in _spawned) if (it) Destroy(it.gameObject);
            _spawned.Clear();
        }

        private void PopulateInventory()
        {
            if (database == null || inventoryGrid == null || itemPrefab == null) return;

            ClearInventory();

            IEnumerable<MechComponentSO> items = Enumerable.Empty<MechComponentSO>();
            if (mode == SelectMode.Structural)
            {
                items = database.GetStructuralForSlot(selectedSlot);
            }
            else if (mode == SelectMode.WeaponForHardpoint)
            {
                items = database.GetWeapons();
            }

            string q = (searchField ? searchField.text : null);
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.ToLowerInvariant();
                items = items.Where(i => i.DisplayName != null && i.DisplayName.ToLowerInvariant().Contains(q));
            }

            foreach (var comp in items)
            {
                var ui = Instantiate(itemPrefab, inventoryGrid);
                ui.Init(comp, this);
                _spawned.Add(ui);
            }
        }

        public void OnInventoryItemClicked(MechComponentSO comp)
{
    if (comp == null) return;

    if (mode == SelectMode.Structural)
    {
        if (!comp.IsStructural || comp.Slot != selectedSlot)
        {
            Debug.LogWarning($"Cannot equip {comp.DisplayName} into {selectedSlot}.");
            return;
        }
        mech.EquipStructural(comp);
        SelectStructural(selectedSlot); // stay focused
    }
    else if (mode == SelectMode.WeaponForHardpoint)
    {
        if (!comp.IsWeapon)
        {
            Debug.LogWarning($"Cannot mount non-weapon {comp.DisplayName} to {selectedSlot}.");
            return;
        }
        bool ok = mech.MountWeapon(selectedSlot, selectedHardpointIndex, comp);
        if (!ok) Debug.LogWarning($"Failed to mount {comp.DisplayName} to {selectedSlot} HP {selectedHardpointIndex+1}");
        // stay in weapon mode for rapid testing
        SelectWeaponForSlot(selectedSlot, selectedHardpointIndex);
    }

    RefreshPaperDoll();
}


        private void RefreshPaperDoll()
        {
            foreach (var s in AllSlotUIs())
                if (s != null) s.Refresh(mech);
        }
    }
}
