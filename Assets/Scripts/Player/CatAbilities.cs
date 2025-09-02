using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatAbilities : MonoBehaviour
{
    [SerializeField]
    private List<CatAbilitySO> AbilityList;
    public int HasGlobalAbility
    {
        get; private set;
    }
    public void AddAbility()
    {
        var seed = Random.Range(0, 100);
        //TODO - come up with drop table and abilities, later.
    }

}
