using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//variable to store the state of the battle
public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHUD playerHUD;
    [SerializeField] BattleHUD enemyHUD;
    [SerializeField] BattleDialogBox dialogBox;

    public event Action <bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove; /*if current action is 0 -> "Fight" will be selected
                                            1 -> "Run" will be selected */

    //show in private variables
    PokemonParty playerParty; 
    PokemonLevel wildPokemon; 
    public void StartBattle(PokemonParty playerParty, PokemonLevel wildPokemon) /*pass the player's party and the wild pokemon
                                                                                  while calling the StartBattle() function */
    {
        //assign parameters to variables
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyPokemon()); //create player's Pokemon that aren't fainted
        enemyUnit.Setup(wildPokemon);  //create opponents's Pokemon
        playerHUD.SetData(playerUnit.Pokemon);
        enemyHUD.SetData(enemyUnit.Pokemon);

        dialogBox.SetMovesName(playerUnit.Pokemon.Moves); //passing the moves of Pokemon to the SetMovesName function

       /* use yield return statement to execute another coroutine -> this will wait the "dialogBox.TypeDialog(...)" coroutine to be completed
        and only after that, the execution will come down */
       yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");

       PlayerAction(); //let the player select action
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("Choose an action!"));
        dialogBox.EnableActionSelector(true); //enable action after using public function in BattleDialogBox
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //player's pokemon will perform attack or the moves and enemy's pokemon will take damage from that attack
    IEnumerator PerformPlayerMove() 
    {
        state = BattleState.Busy;/* because if the state is in player move, player will still be able to 
                                    change the value of the current move */

        var move = playerUnit.Pokemon.Moves[currentMove]; //reference to the move that was selected by player
        move.PP--; //reduce PP when using a move
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used  {move.Base.Name}!"); //dialog box show that player's pokemon used the move

        playerUnit.PlayAttackAnimation(); //attack animation
        yield return new WaitForSeconds(1f); //wait a secon before starting to reduce HP

        enemyUnit.PlayHitAnimation(); //when player attack, enemy SHOULD take the hit

        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon); /* apply damage to enemy's pokemon 
                                                                                    and will return whether the enemy's pokemon fainted or not */
        yield return enemyHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if (damageDetails.Fainted) 
        {
            yield return dialogBox.TypeDialog($"The wild {enemyUnit.Pokemon.Base.Name} fainted!");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog($"You have won the battle!");

            yield return new WaitForSeconds(1f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove()); //not fainted -> enemy will attack now
        }
    }
    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"The wild {enemyUnit.Pokemon.Base.Name} used {move.Base.Name}!");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation(); //when enemy attack, player SHOULD take the hit

        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return playerHUD.UpdateHP();
        yield return ShowDamageDetails(damageDetails); 

        if (damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"Your {playerUnit.Pokemon.Base.Name} has fainted!");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            //check if there is any other healthy Pokemon in the party
            var nextPokemon = playerParty.GetHealthyPokemon();

            if (nextPokemon != null) //if there is an actual healthy Pokemon in party, send it out for battle
            {
                playerUnit.Setup(nextPokemon); 
                playerHUD.SetData(nextPokemon);
                
                dialogBox.SetMovesName(nextPokemon.Moves); 

                yield return dialogBox.TypeDialog($"Go {nextPokemon.Base.Name}!");

                PlayerAction();
            }
            else
            {
                OnBattleOver(false);
            }

            
        }
        else
        {
            PlayerAction();
        }
    }

    //function to show the damage details in the dialog box
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
         yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective!");
        else if (damageDetails.TypeEffectiveness == 0)
            yield return dialogBox.TypeDialog("It has no effect!");
    }

    public void HandleUpdate() //same as PlayerController
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }
    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) 
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentAction > 0)
                --currentAction;
        }
        dialogBox.UpdateActionSelection(currentAction);
         
        if (Input.GetKeyDown(KeyCode.Z)) // Z key is for choosing the option
        {
            if (currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Run
            }
        }
    }
    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count -1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 1)
                currentMove -=2;
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) // Z key is pressed
        {
            dialogBox.EnableMoveSelector(false); //disable move selector
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove()); //call coroutine that will perform the move
        }
    }
} 
