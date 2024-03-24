using System;
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
    PokemonParty party;
    
    int selection = 0;

    public Pokemon SelectedMember => pokemons[selection];
    
    //Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom { get; set; }
    public void Init()
    {
        /*this function will return all the party member UI Components that are attached to the child objects of the party screen
        => Don't have to assign one-by-one from the inspector and don't have to worry in case changing number of members that allow in party */
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);

        party = PokemonParty.GetPlayerParty();
        SetPartyData();
        
        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        pokemons = party.Pokemons;

        for  (int i = 0; i < memberSlots.Length; i++)
        {
            if (i < pokemons.Count) 
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(pokemons[i]); //set the data of pokemon into the member slot and passing pokemon at that index
            } 
            else
                memberSlots[i].gameObject.SetActive(false);
        }
        
        UpdateMemberSelection(selection);

        messageText.text = "Choose a Pokemon!";
    }
    
    public void HandleUpdate(Action onSelected, Action onBack) //responsible for selecting pokemons
    {
        var previousSelection = selection;
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
            selection++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            selection--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);
        
        if (selection != previousSelection)
            UpdateMemberSelection(selection);
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }

    public void UpdateMemberSelection(int selectedMember) //take index of the selected member
    {
        for (int i = 0; i < pokemons.Count; i++) //loop through all pokemons
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
            
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
