using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Cat : MonoBehaviour
{
    public int Level;
    public int ExpToNextLevelAbs;
    public int CurrentExp; //each level is 10x Level exp.
    public CatAbilities Abilities; //1 ability per 10 levels scaling infinitely.
    public float critChance;
    // Start is called before the first frame update
    public void GainExp(int exp)
    {
        CurrentExp += exp;
        if (CurrentExp >= ExpToNextLevelAbs)
        {
            int newExp = CurrentExp - ExpToNextLevelAbs;
            CurrentExp = 0;
            LevelUp();
            GainExp(newExp);
        }
    }

    public void LevelUp()
    {
        Level += 1;
        if (Level % 10 == 0)
        {
            Abilities.AddAbility();
        }
    }
}
