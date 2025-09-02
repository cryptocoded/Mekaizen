using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Cat Abilities/Ability")]
public class CatAbilitySO : ScriptableObject
{
    [Header("Name")]
    [SerializeField] private string DisplayName;

    [Header("Basic Ability")]
    [SerializeField]
    [Tooltip("Basic Abilities affect the cat that has the ability.")]
    [TextArea] private string Description;
    [SerializeField] private int DamageBoost;
    [SerializeField] private int PowerGenBoost;
    [SerializeField] private int SpeedBoost;
    [SerializeField] private int ArmorBoost;
    [SerializeField] private int ComputationBoost;
    [Header("Level")]
    [Tooltip("Level is increased when a cat gains the same ability multiple times. Each level in an ability doubles its effectiveness (x*2^(Level-1))")]
    [SerializeField] private int Level;

    [Header("External Ability")]
    [SerializeField]
    [Tooltip("External Abilities affect other cats working on the same task as the cat with the external ability.")]
    [TextArea] private string ExternalDescription;
    
    /// <summary>
    /// Checks if there is an external ability description to determine if this is an external ability.
    /// </summary>
    /// <returns>True if ExternalDescription contains text, false otherwise.</returns>
    public bool IsExternalAbility()
    {
        return ExternalDescription != null;
    }

    [Header("Global Effect")]
    [SerializeField]
    [Tooltip("Global Effects affect all cats.")]
    [TextArea] private string GlobalDescription;
    //TODO - global stats and requirements.

    /// <summary>
    /// Checks if a global description has been entered to know if the ability is a global ability.
    /// </summary>
    /// <returns>True if there is a global description, false otherwise</returns>
    public bool IsGlobalAbility()
    {
        return GlobalDescription != null;
    }

    [Header("Chance Ability")]
    [Tooltip("Chances should always add up to 1. Number of chance entries should equal ability entries.")]
    [SerializeField]
    private List<float> Chances;

    [SerializeField]
    [Tooltip("Number of Abilities should equal number of chance entries.")]
    private List<CatAbilitySO> Abilities;

    /// <summary>
    /// Checks if both Chances and Abilities are non-null.
    /// </summary>
    /// <returns>True if both Chances and Abilities are not null, False if either is null.</returns>
    private bool IsChanceAbility()
    {
        return Chances != null && Abilities != null;
    }


}