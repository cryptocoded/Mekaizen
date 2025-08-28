using System.Collections.Generic;
using Mechs;
using UnityEngine;

public class StructuralComponentSO : MechComponentSO
{
    public int AvailableHardpoints
    {
        get; private set;
    }

    public List<EquipComponentSO> AllEquips;

    public MechComponentSO Armor
    {
        get;
    }

    public MechComponentSO Reactor
    {
        get;
    }

    public MechComponentSO System
    {
        get;
    }

    public bool CanEquip(EquipComponentSO equip)
    {
        if (equip.RequiredHardpoints <= AvailableHardpoints)
        {
            return true;
        }
        return false;
    }

    public void EquipEquipable(EquipComponentSO equip)
    {
        if (CanEquip(equip)) 
        { 
            AllEquips.Add(equip);
            AvailableHardpoints -= equip.RequiredHardpoints;
        }
        else
        {
            Debug.LogWarning($"EquipEquipable failed: Not enough sockets to equip {equip}.");
        }
        return;
    }

    public void RemoveEquipable(EquipComponentSO equip)
    {
        if(AllEquips.Contains(equip))
        {
            AllEquips.Remove(equip);
            AvailableHardpoints += equip.RequiredHardpoints;
        }
        else
        {
            Debug.LogWarning($"RemoveEquipable failed: {equip} is not equipped.");
        }
    }
}