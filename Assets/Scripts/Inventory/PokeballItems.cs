using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new Pokeball ")]
public class PokeballItems : ItemBase
{
    [SerializeField] private float catchRateModifier = 1;
    
    public override bool Use(Pokemon pokemon)
    {
        if (GameController.Instance.State == GameState.Battle)
            return true;
        
        return false;
    }

    public float CatchRateModifier => catchRateModifier;
}
