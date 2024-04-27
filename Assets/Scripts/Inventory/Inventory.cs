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
public class Inventory : MonoBehaviour, ISavable
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
            RemoveItem(item);
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        int category = (int)GetCatergoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        var itemSlot = currentSlots.FirstOrDefault(slot => slot.Item == item);
        if (itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlots.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        OnUpdated?.Invoke();
    }
    
    public void RemoveItem(ItemBase item)
    {
        int category = (int)GetCatergoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);
        
        var itemSlot = currentSlots.First(slot => slot.Item == item);
        itemSlot.Count--;
        if (itemSlot.Count == 0)
            currentSlots.RemoveAt(itemSlot.Count);
        
        OnUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int category = (int)GetCatergoryFromItem(item);
        var currentSlots = GetSlotsByCategory(category);

        return currentSlots.Exists(slot => slot.Item == item);
    }

    ItemCategory GetCatergoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
            return ItemCategory.ITEMS;
        else 
            return ItemCategory.POKEBALLS;
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new ItemSlot.InventorySaveData()
        {
            items = slots.Select(i => i.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(i => i.GetSaveData()).ToList(),
        };
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = state as ItemSlot.InventorySaveData;
        
        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(i => new ItemSlot(i)).ToList();
        
        allSlots = new List<List<ItemSlot>>() {slots, pokeballSlots};

        OnUpdated?.Invoke();
    }
}

[Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {
        
    }
    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetItemByName(saveData.name);
        count = saveData.count;
        
    }
    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.Name,
            count = count
        };
        return saveData;
    }
    public ItemBase Item
    {
        get => item;  
        set => item = value;
    }

    public int Count
    {
        get => count;
        set => count = value;
    }

    [Serializable]
    public class ItemSaveData
    {
        public string name;
        public int count;
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<ItemSaveData> items;
        public List<ItemSaveData> pokeballs;
    }
}
