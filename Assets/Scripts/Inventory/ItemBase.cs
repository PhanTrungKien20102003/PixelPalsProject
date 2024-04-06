using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    [SerializeField] Sprite icon;
    
    public string Name => name;
    public string Description => description;
    public Sprite Icon => icon;

    
    //this function will indicate whether the item is used or not
    //sometimes can't use the item. For example, when the Pokemon is full HP, can't use the potion item
    public virtual bool Use(Pokemon pokemon) //not have any implementation in the ItemBase class but will be implemented in the subclass
    {
        return false;
    }
}
