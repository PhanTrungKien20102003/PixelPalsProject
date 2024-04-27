using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDB : MonoBehaviour
{
    static Dictionary<string, ItemBase> items;

    public static void Init()
    {
        items = new Dictionary<string, ItemBase>();
        
        //this function will only load the objects that are inside a folder call Resources
        var itemList = Resources.LoadAll<ItemBase>("");
        foreach (var item in itemList)
        {
            if (items.ContainsKey(item.Name))
            {
                Debug.LogError($"There is already a item called {item.Name}!");
                continue;
            }
            
            items[item.Name] = item;
        }
    }
    public static ItemBase GetItemByName(string name)
    {
        if (!items.ContainsKey(name))
        {
            Debug.LogError($"Item with the name {name} not found in database!");
            return null;
        }
        return items[name];
    }
}
