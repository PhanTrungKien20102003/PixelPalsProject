using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI countText;

    public TextMeshProUGUI NameText => nameText;
    public TextMeshProUGUI CountText => countText;
    public void SetData(ItemSlot itemSlot)
    {
        nameText.text = itemSlot.Item.name;
        countText.text = $"X {itemSlot.Count}";
    }
}
