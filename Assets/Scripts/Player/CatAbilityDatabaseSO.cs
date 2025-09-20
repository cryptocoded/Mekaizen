using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Cat Abilities/Database")]
public class CatAbilityDatabseSO : ScriptableObject
{
    [SerializeField]
    private List<CatAbilities> CommonAbilities = new();

    [SerializeField]
    private List<CatAbilities> UncommonAbilities = new();

    [SerializeField]
    private List<CatAbilities> RareAbilities = new();

    [SerializeField]
    private List<CatAbilities> LegendaryAbilities = new();
}