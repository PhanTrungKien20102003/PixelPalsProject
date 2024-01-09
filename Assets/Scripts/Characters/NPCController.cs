using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog; //show dialog when talking to NPCs
    
    [SerializeField] List<Vector2> movementPattern; //specify the pattern

    [SerializeField] float timeBetweenPattern; //set time between the pattern from inspector

    NPCState state;
    float idleTimer = 0f; //keep track the time when NPC walk
    int currentPattern = 0;

    Character charater;
    private void Awake()
    {
        charater = GetComponent<Character>();
    }
    public void Interact()
    {
        //can only interact with NPC when the NPC is in the Idle state, won't be able to talk when walking
        if (state == NPCState.Idle)
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }
    private void Update()
    {
        if (DialogManager.Instance.IsShowing) //if the dialog is currently being shown -> none of the code below will be executed
            return;

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
        charater.HandleUpdate();
    }

    IEnumerator Walk()
    {
        state = NPCState.Walking;

        yield return charater.Move(movementPattern[currentPattern]); //moving the NPC
        currentPattern = (currentPattern + 1) % movementPattern.Count; //when reach the last pattern, go back to the first pattern

        state = NPCState.Idle;
    }

    public enum NPCState //make the NPCs walk in a pattern that I specify
    {
        Idle,
        Walking
    }
}
