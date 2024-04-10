using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ItemCategory
{
    ITEMS,
    POKEBALLS
}
public class Inventory : MonoBehaviour
{ 
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    
    //store all the slots that available
    List <List<ItemSlot>> allSlots;
    public event Action OnUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>>() {slots, pokeballSlots};
    }
    public static List<string> ItemCategories { get; set; } = new List<string>()
    {
        "ITEMS","POKEBALLS"
    };

    //this function should return one of those two lists based on the category index
    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var currentSlots = GetSlotsByCategory(selectedCategory);
        
        var item = currentSlots[itemIndex].Item;
        bool itemUsed = item.Use(selectedPokemon);
        if (itemUsed)
        {
            RemoveItem(item, selectedCategory);
            return item;
        }

        return null;
    }

    public void RemoveItem(ItemBase item, int category)
    {
        var currentSlots = GetSlotsByCategory(category);
        
        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
            currentSlots.RemoveAt(itemSlot.Count);
        
        OnUpdated?.Invoke();
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemBase Item => item;

    public int Count
    {
        get => count;
        set => count = value;
    }
}
