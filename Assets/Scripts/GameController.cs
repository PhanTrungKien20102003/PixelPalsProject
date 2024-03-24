using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*State Design Pattern
    - Have a game controller inside which will store the GameState -- GameState = FreeRoam, Battle, etc.
        +) Depending upon the state, we will give the controller to either Player Controller or Battle System 
        => Both of them can't have the control at the same time. */
public enum GameState { FreeRoam, Battle, Dialog, Menu, PartySlot, Bag, CutScene, Paused}
public class GameController : MonoBehaviour
{
    //references for both Player and Battle
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;

    [SerializeField] Camera worldCamera; //reference for main camera

    [SerializeField] PartySlot partySlot; //reference for party slot
    
    [SerializeField] InventoryUI inventoryUI;
    
    GameState state;
    
    GameState stateBeforePause;
    
    MenuController menuController;
    public static GameController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        PokemonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;
        
        partySlot.Init();
        
        DialogManager.Instance.OnShowDialog += () => //subscribe to the OnShowDialog event 
        {
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnCloseDialog += () => //subscribe to the OnCloseDialog event 
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };
        
        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };
        menuController.onMenuSelected += OnMenuSelected;
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }
    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = FindObjectOfType<MapArea>().GetComponent<MapArea>().GetRandomWildPokemon();

        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemon); //will be called everytime encountered a new battle
    }

    private TrainerController trainer;
    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty); //will be called everytime encountered a new battle
    }

    public void OneEnterTrainersView(TrainerController trainer)
    {
        state = GameState.CutScene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }
    void EndBattle(bool won)
    {
        if (trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }
        
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

            if (Input.GetKeyDown(KeyCode.Return))
            {
                menuController.OpenMenu();
                state = GameState.Menu;
            }
        }
        else if (state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if (state == GameState.Dialog) //disable player movement when talking to NPCs (show dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if (state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if (state == GameState.PartySlot)
        {
            Action onSelected = () =>
            {
                //go to summary screen
            };
            Action onBack = () =>
            {
                partySlot.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            
            partySlot.HandleUpdate(onSelected, onBack);
        }
        else if (state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            inventoryUI.HandleUpdate(onBack);
        }
    }
    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            //Pokemon
            partySlot.gameObject.SetActive(true);
            state = GameState.PartySlot;
        }
        else if (selectedItem == 1)
        {
            //Bag
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            //Save
            SavingSystem.instance.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            //Load
            SavingSystem.instance.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }
}
