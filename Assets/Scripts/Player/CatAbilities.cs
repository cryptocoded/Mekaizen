using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAbilities : MonoBehaviour
{
    public List<Mechs.SpecialEffectSO> AbilityList;
    public bool HasGlobalAbility;
    public void AddAbility()
    {
        var seed = Random.Range(0, 100);
        //TODO - come up with drop table and abilities, later.
    }
}
