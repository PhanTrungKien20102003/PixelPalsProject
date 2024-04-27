using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog; //show dialog when talking to NPCs
    
    [Header("Quests")]
    [SerializeField] QuestBase questToStart; //start this quest if player interact with NPC
    [SerializeField] QuestBase questToComplete; //complete the quest
    
    [Header("Movement")]
    [SerializeField] List<Vector2> movementPattern; //specify the pattern
    [SerializeField] float timeBetweenPattern; //set time between the pattern from inspector

    NPCState state;
    float idleTimer = 0f; //keep track the time when NPC walk
    int currentPattern = 0;
    Quest activeQuest;

    Character character;
    ItemGiver itemGiver;
    PokemonGiver pokemonGiver;
    private void Awake()
    {
        character = GetComponent<Character>();
        itemGiver = GetComponent<ItemGiver>();
        pokemonGiver = GetComponent<PokemonGiver>();
    }

                        //the Transform of the Game Object that initiated the interaction. In this case, it's the transform of the player
    public IEnumerator Interact(Transform initiator)
    {
        //can only interact with NPC when the NPC is in the Idle state, won't be able to talk when walking
        if (state == NPCState.Idle)
        {
            state = NPCState.Dialog;
            character.LookTowards(initiator.position);

            if (questToComplete != null)
            {
                var quest = new Quest(questToComplete);
                yield return quest.CompleteQuest(initiator);
                questToComplete = null;

                Debug.Log($"{quest.Base.Name} completed!");
            }
            
            if (itemGiver != null && itemGiver.CanBeGiven())
            {
                yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
            }
            else if (pokemonGiver != null && pokemonGiver.CanBeGiven())
            {
                yield return pokemonGiver.GivePokemon(initiator.GetComponent<PlayerController>());
            }
            else if (questToStart != null)
            {
                activeQuest = new Quest(questToStart);
                yield return activeQuest.StartQuest();
                questToStart = null;
                
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
            }
            else if (activeQuest != null)
            {
                if (activeQuest.CanBeCompleted())
                {
                    yield return activeQuest.CompleteQuest(initiator);
                    activeQuest = null;
                }
                else
                {
                    yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialog);
                }
            }
            else
            {
                yield return DialogManager.Instance.ShowDialog(dialog);
            }
            
            idleTimer = 0f; //set to 0 to not use any previously stored value
            state = NPCState.Idle;
            
        }
    }
    private void Update()
    {
        if (state == NPCState.Idle)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeBetweenPattern)
            {
                idleTimer = 0;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        character.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        /*if the path is not clear -> NPC must not skip to the next pattern
         => if it skips a pattern, it'll walk in a pattern that different from the one specified 
                => don't have control over where all the NPCs can go */

        var oldPos = transform.position; //check if NPC actually walked in the previous pattern

        yield return character.Move(movementPattern[currentPattern]); //moving the NPC

        if (transform.position != oldPos)
            currentPattern = (currentPattern + 1) % movementPattern.Count; //when reach the last pattern, go back to the first pattern

        state = NPCState.Idle;
    }

    public enum NPCState //make the NPCs walk in a pattern that I specify
    {
        Idle,
        Walking,
        Dialog
    }
}
