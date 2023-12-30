using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartySlot : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    
    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    public void Init()
    {
        /*this function will return all the party member UI Components that are attached to the child objects of the party screen
        => Don't have to assign one-by-one from the inspector and don't have to worry in case changing number of members that allow in party */
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List <Pokemon> pokemons)
    {
        this.pokemons = pokemons;

        for  (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count) 
            {
                memberSlots[i].SetData(pokemons[i]); //set the data of pokemon into the member slot and passing pokemon at that index
            } else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        messageText.text = "Choose a Pokemon!";
    }

    public void UpdateMemberSelection(int selectedMember) //take index of the selected member
    {
        for (int i = 0; i < pokemons.Count ;i++) //loop through all pokemons
        {
            if (i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            } else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
