using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*State Design Pattern
    - Have a game controller inside which will store the GameState -- GameState = FreeRoam, Battle, etc.
        +) Depending upon the state, we will give the controller to either Player Controller or Battle System 
        => Both of them can't have the control at the same time. */
public enum GameState { FreeRoam, Battle}
public class GameController : MonoBehaviour
{
    //references for both Player and Battle
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;

    [SerializeField] Camera worldCamera; //reference for main camera

    GameState state;

    private void Start()
    {
        playerController.OnEncountered += StartBattle; /* subscribed to the event that has created and called new function
                                                         "StartBattle" when this event is fired */
        battleSystem.OnBattleOver += EndBattle; 
    }
    void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        battleSystem.StartBattle(playerParty, wildPokemon); //will be called everytime encountered a new battle
    }
    void EndBattle(bool won)
    {
        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }
    private void Update()
    {
        if (state == GameState.FreeRoam) /* if the state is FreeRoam, give the control to the Player Controller
                                            otherwise, if the state is Battle, give the control to Battle System */
        {
            playerController.HandleUpdate();
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
    }
}
