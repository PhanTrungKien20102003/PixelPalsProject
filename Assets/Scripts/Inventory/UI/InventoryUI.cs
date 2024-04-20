using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public enum InventoryUIState { ItemSelection, PartySelection, Busy}

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject itemList;
    [SerializeField] ItemSlotUI itemSlotUI;

    [SerializeField] TextMeshProUGUI categoryText;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemDescription;

    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;

    [SerializeField] PartySlot partySlot;

    Action<ItemBase> onItemUsed;
    
    int selectedItem = 0;
    int selectedCategory = 0;
    
    InventoryUIState state;
    
    const int itemsInViewport = 9;

    List<ItemSlotUI> slotUIList; //the list to hold all the instantiated items 
    Inventory inventory;
    RectTransform itemListRect;
    
    private void Awake()
    {
        inventory = Inventory.GetInventory();
        itemListRect = itemList.GetComponent<RectTransform>();
    }

    private void Start()
    {
        UpdateItemList();
        
        inventory.OnUpdated += UpdateItemList;
    }

    void UpdateItemList()
    {
        //Clear all existing items
        foreach (Transform child in itemList.transform)
            Destroy(child.gameObject);

        slotUIList = new List<ItemSlotUI>();
        foreach (var itemSlot in inventory.GetSlotsByCategory(selectedCategory))
        {
            var slotUIObject = Instantiate(itemSlotUI, itemList.transform);
            slotUIObject.SetData(itemSlot);
            
            slotUIList.Add(slotUIObject);
        }
        
        UpdateItemSelection();
    }

    public void HandleUpdate(Action onBack, Action<ItemBase> onItemUsed = null)
    {
        this.onItemUsed = onItemUsed;
        
        if (state == InventoryUIState.ItemSelection)
        {
            int previousSelection = selectedItem;
            int previousCategory = selectedCategory;
            
            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedItem++;
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedItem--;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                selectedCategory++;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                selectedCategory--;

            if (selectedCategory > Inventory.ItemCategories.Count - 1)
                selectedCategory = 0;
            else if (selectedCategory < 0)
                selectedCategory = Inventory.ItemCategories.Count - 1;

            selectedItem = Mathf.Clamp(selectedItem, 0, inventory.GetSlotsByCategory(selectedCategory).Count - 1);

            if (previousCategory != selectedCategory)
            {
                ResetSelection();
                categoryText.text = Inventory.ItemCategories[selectedCategory];
                UpdateItemList();
            } 
            else if (previousSelection != selectedItem)
            {
                UpdateItemSelection();
            }

            if (Input.GetKeyDown(KeyCode.Z))
                ItemSelected();
            else if (Input.GetKeyDown(KeyCode.X))
                onBack?.Invoke();
        }
        else if (state == InventoryUIState.PartySelection)
        {
            Action onSelected = () =>
            {
                //Use the item on the selected Pokemon
                StartCoroutine(UseItem());
            };
            Action onBackPartyScreen = () =>
            {
                ClosePartySlot();
            };
            //Handle party selection
            partySlot.HandleUpdate(onSelected, onBackPartyScreen);
        }
    }

    void ItemSelected()
    {
        if (selectedCategory == (int)ItemCategory.POKEBALLS)
        {
            StartCoroutine(UseItem());
        }
        else
        {
            OpenPartySlot();
        }
    }

    IEnumerator UseItem()
    {
        state = InventoryUIState.Busy;
        
        var usedItem = inventory.UseItem(selectedItem, partySlot.SelectedMember, selectedCategory); 
        if (usedItem != null)
        {
            if (!(usedItem is PokeballItems))
                yield return DialogManager.Instance.ShowDialogText($"The player used {usedItem.Name}!");
                
            onItemUsed?.Invoke(usedItem);
        }
        else
        {
            if (selectedCategory == (int)ItemCategory.ITEMS)
                yield return DialogManager.Instance.ShowDialogText($"It won't have any effect!");
        }

        ClosePartySlot();
    }
    
    void UpdateItemSelection()
    {
        var slots = inventory.GetSlotsByCategory(selectedCategory);
        
        selectedItem = Mathf.Clamp(selectedItem, 0, slots.Count - 1);

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i == selectedItem)
                slotUIList[i].NameText.color = GlobalSettings.instance.HighlightedColor;
            else
                slotUIList[i].NameText.color = Color.black;
        }
        
        if (slots.Count > 0)
        {
            var item = slots[selectedItem].Item;
            itemIcon.sprite = item.Icon;
            itemDescription.text = item.Description;
        }
        
        HandleScrolling();
    }

    //this function is for when user choose the first 4 item, it won't start scrolling but when user choose the 5th item, the scroll will start
    void HandleScrolling()
    {
        if (slotUIList.Count <= itemsInViewport)
            return;
        
        float scrollPosition = Mathf.Clamp(selectedItem - itemsInViewport/2, 0, selectedItem) * slotUIList[0].Height;
        itemListRect.localPosition = new Vector2(itemListRect.localPosition.x, scrollPosition);

        bool showUpArrow = selectedItem > itemsInViewport / 2;
        upArrow.gameObject.SetActive(showUpArrow);
        
        bool showDownArrow = selectedItem + itemsInViewport / 2 < slotUIList.Count;
        downArrow.gameObject.SetActive(showDownArrow);

    }

    void ResetSelection()
    {
        selectedItem = 0;
        
        upArrow.gameObject.SetActive(false);
        downArrow.gameObject.SetActive(false);

        itemIcon.sprite = null;
        itemDescription.text = "";

    }
    void OpenPartySlot()
    {
        state = InventoryUIState.PartySelection;
        partySlot.gameObject.SetActive(true);
    }
    void ClosePartySlot()
    {
        state = InventoryUIState.ItemSelection;
        partySlot.gameObject.SetActive(false);
    }
}
