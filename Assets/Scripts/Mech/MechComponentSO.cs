using System.Collections.Generic;
using Mechs;
using UnityEngine;

// ... existing using + namespace Mechs
[CreateAssetMenu(menuName = "Mechs/Component")]
public class MechComponentSO : ScriptableObject
{
    public string DisplayName;

    [Header("Classification")]
    public ComponentType Type;
    public SlotType Slot = SlotType.None;

    [Header("Mounting")]
    public HardpointsManager HardpointsManager;

    public int RequiredHardpoints = 0;

    [Header("Visuals")]
    public Sprite Icon; // <— NEW: optional UI icon

    [Header("Tags & Stats")]
    public List<string> Tags = new List<string>();
    public List<StatEntry> BaseStats = new List<StatEntry>();
    public List<SpecialEffectSO> Effects = new List<SpecialEffectSO>();

    public bool HasTag(string tag) => Tags != null && Tags.Contains(tag);
    public bool IsStructural => Slot != SlotType.None;
//this is really "attaches to a hardpoint," buuutttt
    public bool IsWeapon => Type == ComponentType.Weapon;
}

