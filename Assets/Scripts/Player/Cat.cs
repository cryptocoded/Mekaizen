using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Cat : MonoBehaviour
{
    [SerializeField]
    private int Level;
    private int CurrentExp; //each level is 10x Level exp.
    private CatAbilities Abilities; //1 ability per 10 levels scaling infinitely.
    private float Morale;
    // Start is called before the first frame update
    public void GainExp(int exp)
    {
        CurrentExp += exp;
        if (CurrentExp >= TotalExpToNextLevel())
        {
            int newExp = CurrentExp - TotalExpToNextLevel();
            CurrentExp = 0;
            LevelUp();
            GainExp(newExp);
        }
    }

    public int TotalExpToNextLevel()
    {
        return Level * 10;
    }

    private void LevelUp()
    {
        Level += 1;
        if (Level % 10 == 0)
        {
            Abilities.AddAbility();
        }
    }

    public float GetCritChance()
    {
        return 0;
    }

    public int GetEffectiveLevel(/*type of upgrade*/)
    {
        return Level;
    }
}
