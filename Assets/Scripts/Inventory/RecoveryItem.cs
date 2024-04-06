using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount; //for Potion (heal for specific amount of health)
    [SerializeField] bool restoreMaxHP; //for Max Potion (heal to maximum health)
    
    [Header("PP")]
    [SerializeField] int ppAmount; //for Ether (restore 10 PP of move)
    [SerializeField] bool restoreMaxPP; //for Max Ether (restore max PP of moves)

    [Header("Status Conditions")]
    [SerializeField] ConditionID status; //for different Potion (cure status like Para,Burn,Poison,...)
    [SerializeField] bool recoverAllStatus; //to cure all status (eg. Para + Confused,...)
    
    [Header("Revive")]
    [SerializeField] bool revive; //for getting the Pokemon back if they fainted to 1/2 HP in the middle or after battle
    [SerializeField] bool maxRevive; //getting the Pokemon from fainted back to full HP

    public override bool Use(Pokemon pokemon)
    {
        // Revive
        if (revive || maxRevive)
        {
            if (pokemon.HP > 0)
                return false;
            
            if (revive)
                pokemon.IncreaseHP(pokemon.MaxHp / 2);
            else if (maxRevive)
                pokemon.IncreaseHP(pokemon.MaxHp);
            
            pokemon.CureStatus();

            return true;
        }

        // No other items can be used on fainted Pokemon
        if (pokemon.HP == 0)
            return false;
        
        // Restore HP
        if (restoreMaxHP || hpAmount > 0)
        {
            if (pokemon.HP == pokemon.MaxHp) //this is the case for when Pokemon HP is already full => don't want to use potion on that Pokemon
                return false;
            
            if (restoreMaxHP)
                pokemon.IncreaseHP(pokemon.MaxHp);
            else
                pokemon.IncreaseHP(hpAmount);
        }
        
        // Recover status
        if (recoverAllStatus || status != ConditionID.None)
        {
            if (pokemon.Status == null && pokemon.VolatileStatus != null)
                return false;
            if (recoverAllStatus)
            {
                pokemon.CureStatus();
                pokemon.CureVolatileStatus();
            }
            else
            {
                if (pokemon.Status.id == status)
                    pokemon.CureStatus();
                else if (pokemon.VolatileStatus.id == status)
                    pokemon.CureVolatileStatus();
                else
                    return false;
            }
        }
        
        //Restore PP
        if (restoreMaxPP)
        {
            pokemon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if (ppAmount > 0)
            pokemon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        return true;
    }
}

