using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Cats : MonoBehaviour
{
    public List<Cat> CatList;

    public int EffectiveLevel(/*type of upgrade?*/)
    {
        int effectiveLevel = 0;
        foreach (var cat in CatList)
        {
            effectiveLevel += cat.GetEffectiveLevel();
            //TODO - apply ability bonuses
            //maybe make a cat function that gets the effective level given the type of upgrade.
        }
        return effectiveLevel;
    }

    public float CriticalChance(/*type of upgrade?*/)
    {
        float critChance = 0;
        foreach (var cat in CatList)
        {
            critChance += cat.GetCritChance();
            //TODO - same as above
        }
        return critChance;
    }
}
