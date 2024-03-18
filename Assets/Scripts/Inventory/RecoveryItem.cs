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
}
