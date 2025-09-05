using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

[CreateAssetMenu(menuName = "Cat Abilities/Ability")]
public class CatAbilitySO : ScriptableObject
{


    [Header("Name")]
    [SerializeField] private string DisplayName;

    [Header("Ability Fields")]
    [SerializeField]
    [Tooltip("Make sure the description matches the ability kind and is worded such that it is obvious what kind of ability this is.")]
    [TextArea] private string Description;
    [Tooltip("If set to true this boost will be a percent multiplier rather than a flat level boost. e.g. \"10\" would be Level * 1.1 instead of Level + 10.")]
    [SerializeField] private bool isPercentBoost;
    [SerializeField] private int DamageBoost;
    [SerializeField] private int PowerGenBoost;
    [SerializeField] private int SpeedBoost;
    [SerializeField] private int ArmorBoost;
    [SerializeField] private int ComputationBoost;
    [Tooltip("Chance that when completing a task you also gain a cat box.")]
    [SerializeField] private float CatChance;

    [Header("Level")]
    [Tooltip("Level is increased when a cat gains the same ability multiple times. Each level in an ability doubles its effectiveness (x*2^(Level-1))")]
    [SerializeField] private int Level;

    [Header("Restrictions")]
    [Tooltip("The number of other cats required on the same task as the ability holder for this ability to activate. Modified by RestrictionExpression")]
    [SerializeField] private int NumOtherCats;

    [Tooltip("The expression to compare NumOtherCats to the cats on a task with")]
    [SerializeField] private Operator NumCatsComparisonOperator;
    [Tooltip("Some abilities only activate if the total effective level on a task is above or below a certain threshold")]
    [SerializeField] private int TotalEffectiveLevel;
    [Tooltip("The expression to compare TotalEffectiveLevel to the effective level of the cats on the task.")]
    [SerializeField] private Operator TotalEffectiveLevelComparisonOperator;

    [Header("Kind")]
    [Tooltip("The kind of ability changes who the ability affects. Basic Abilities affect only the cat that has the ability" +
        "External Abilities affect other cats on the same task as the cat that has the ability" +
        "Global Abilities affect all cats and " +
        "Chance Abilities have a chance to randomly trigger one or more random effects.")]
    [SerializeField] private CatAbilityKind AbilityKind;

    [Header("Chance Ability")]
    [Tooltip("Chances should always add up to 1. Number of chance entries should equal ability entries.")]
    [SerializeField]
    private List<float> Chances;

    [SerializeField]
    [Tooltip("Number of Abilities should equal number of chance entries.")]
    private List<CatAbilitySO> Abilities;

    public enum CatAbilityKind
    {
        Basic = 0,
        External,
        Global,
        Chance
    }

    public enum Operator
    {
        Equals = 0,
        NotEquals,
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo
    }

}