using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

//variable to store the state of the battle
public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartySlot, BattleOver}
public enum BattleAction { Move, SwitchPokemon, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartySlot partySlot;

    public event Action <bool> OnBattleOver;

    BattleState state;
    BattleState? previousState;
    int currentAction;
    int currentMove; /*if current action is 0 -> "Fight" will be selected
                                            1 -> "Run" will be selected */
    int currentMember;

    //show in private variables
    PokemonParty playerParty; 
    Pokemon wildPokemon; 
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) /*pass the player's party and the wild pokemon
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
   
        partySlot.Init();

        dialogBox.SetMovesName(playerUnit.Pokemon.Moves); //passing the moves of Pokemon to the SetMovesName function

       /* use yield return statement to execute another coroutine -> this will wait the "dialogBox.TypeDialog(...)" coroutine to be completed
        and only after that, the execution will come down */
        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");

        ActionSelection(); 
    }


    void BattleOver(bool won) //this function triggers the battle over state and OnBattleOver() event notifies the GameController that the battle has over
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action!");
        dialogBox.EnableActionSelector(true); //enable action after using public function in BattleDialogBox
    }

    void OpenPartySlot()
    {
        state = BattleState.PartySlot;
        partySlot.SetPartyData(playerParty.Pokemons);
        partySlot.gameObject.SetActive(true);
    }
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if (playerAction == BattleAction.Move) //if player choose to perform a move, then I'll run the move of player's pokemon and the enemy's pokemon one by one base on their speed
        {
            playerUnit.Pokemon.CurrentMove = playerUnit.Pokemon.Moves[currentMove];
            enemyUnit.Pokemon.CurrentMove = enemyUnit.Pokemon.GetRandomMove();

            int playerMovePriority = playerUnit.Pokemon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Pokemon.CurrentMove.Base.Priority;

            //Check which pokemon will go first base on Pokemon's Speed, which one higher will move first
            bool playerGoesFirst = true;
            if (enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if (enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed;
            }
          
            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondPokemon = secondUnit.Pokemon;
            //First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Pokemon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }

            //before running the 2nd turn, check if 2nd pokemon was fainted or not
            if (secondPokemon.HP > 0)
            {
                //Second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Pokemon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if (state == BattleState.BattleOver)
                {
                    yield break;
                }
            }                 
        }
        else
        {
            if (playerAction == BattleAction.SwitchPokemon) //player switch Pokemon
            {
                var selectedPokemon = playerParty.Pokemons[currentMember];
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }

            //once the player switch pokemon, end player's turn and the enemy will take the next turn
            var enemyMove = enemyUnit.Pokemon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if (state == BattleState.BattleOver)
            {
                yield break;
            }
        }
        if (state != BattleState.BattleOver)
        {
            ActionSelection();
        }
    }

    //sourceUnit is attacking or performing the move
    //targetUnit is the target for that
    /*RunMove will contain all the logic such as playing animation, taking damage, status effect...*/
    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move) 
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Pokemon);
            yield return sourceUnit.HUD.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Pokemon);

        move.PP--; //reduce PP when using a move
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used  {move.Base.Name}!"); //dialog box show that player's pokemon used the move

        if (CheckIfMoveHit(move, sourceUnit.Pokemon, targetUnit.Pokemon))
        {
            sourceUnit.PlayAttackAnimation(); //attack animation
            yield return new WaitForSeconds(1f); //wait a second before starting to reduce HP

            targetUnit.PlayHitAnimation(); //when player attack, enemy SHOULD take the hit

            if (move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Pokemon, targetUnit.Pokemon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon); /* apply damage to enemy's pokemon 
                                                                                    and will return whether the enemy's pokemon fainted or not */
                yield return targetUnit.HUD.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if (move.Base.Secondary != null && move.Base.Secondary.Count > 0 & targetUnit.Pokemon.HP > 0) //run 2nd effects
            {
                foreach (var secondary in move.Base.Secondary) 
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if (rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Pokemon, targetUnit.Pokemon, secondary.Target);
                    }
                }
            }

            if (targetUnit.Pokemon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} fainted!");
                targetUnit.PlayFaintAnimation();
                yield return new WaitForSeconds(2f);

                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name}'s attack missed!");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Pokemon source, Pokemon target, MoveTarget moveTarget)
    {

        //stat boosting
        if (effects.Boosts != null)
        {
            if (moveTarget == MoveTarget.Player) //apply stat boost
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        //status conditions
        if (effects.Status != ConditionID.None)
        {
            target.SetStatus(effects.Status);
        }

        //volatile status conditions
        if (effects.VolatileStatus != ConditionID.None)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit) //executed after turn and not after move
    {
        if (state == BattleState.BattleOver) //prevent status effect deal damage after the battle over
        {
            yield break;
        }

        //after move, if the state was changed to sthg like party screen the pass the execution and wait for the state to return back to RunningTurn
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

        //status like burn or poison will hurt the pokemon after their turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Pokemon);
        yield return sourceUnit.HUD.UpdateHP();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} fainted!");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    bool CheckIfMoveHit(Move move, Pokemon source, Pokemon target) //this function is for checking the accuracy of pokemon's move, whether it hit or not
    {
        if (move.Base.AlwaysHit) //for move that can not missed, it will skip the accuracy check
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoost[PokemonBase.Stat.Accuracy];
        int evasion = source.StatBoost[PokemonBase.Stat.Evasion];

        var boostValue = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if(accuracy > 0)
        {
            moveAccuracy *= boostValue[accuracy];
        }
        else
        {
            moveAccuracy /= boostValue[-accuracy];
        }

        if (evasion > 0)
        {
            moveAccuracy /= boostValue[evasion];
        }
        else
        {
            moveAccuracy *= boostValue[-evasion];
        }

        //generate random number
        return UnityEngine.Random.Range(1, 101) <= moveAccuracy; //less than or equal to moveAccuracy to make the move hit or miss
    }
    

    IEnumerator ShowStatusChanges(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0) //check if there are any messages inside the status changes queue then show all of the
                                                //in dialog box
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }
    
    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();

            if (nextPokemon != null)
            {
                OpenPartySlot();
            } else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
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
        else if (damageDetails.TypeEffectiveness == 0f)
            yield return dialogBox.TypeDialog("It has no effect!");
    }

    public void HandleUpdate() //same as PlayerController
    {
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartySlot)
        {
            HandlePartySelection();
        }
    }
    void HandleActionSelection() //responsible for selecting actions
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAction++;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow)) { 
            currentAction--;
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentAction += 2;
        } else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentAction -= 2;
        }


        // make sure that the current move variable doesn't go beyond the size of the list
        currentAction = Mathf.Clamp(currentAction, 0, 3);


        dialogBox.UpdateActionSelection(currentAction);
         
        if (Input.GetKeyDown(KeyCode.Z)) // Z key is for choosing the option
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
            }
            if (currentAction == 2)
            {
                //Pokemon
                previousState = state;
                OpenPartySlot();
            }
            else if (currentAction == 3)
            {
                //Run
            }
        }
    }
    void HandleMoveSelection() //responsible for selecting moves
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMove++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMove--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMove -= 2;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z)) // Z key is pressed for choosing options
        {
            var move = playerUnit.Pokemon.Moves[currentMove]; //if pokemon's PP = 0, it can not use that move 
            if (move.PP == 0)
            {
                return;
            }

            dialogBox.EnableMoveSelector(false); //disable move selector
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move)); //call coroutine that will perform the move
        }
        else if (Input.GetKeyDown(KeyCode.X)) // X key is pressed for previous selection
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    void HandlePartySelection() //responsible for selecting pokemons
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentMember++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentMember--;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentMember += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentMember -= 2;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partySlot.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partySlot.SetMessageText("You can't send out a fainted Pokemon!");
                return;
            } 
            if (selectedMember == playerUnit.Pokemon)
            {
                partySlot.SetMessageText("Your Pokemon already in a battle!");
                return;
            }

            partySlot.gameObject.SetActive(false);

            if (previousState == BattleState.ActionSelection) //if the player choose to switch pokemon during a turn => calling run turns
            {
                previousState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else //if the player choose switch pokemon because the current one fainted => directly call SwitchPokemon function
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchPokemon(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partySlot.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon) //if the player actually selected a valid pokemon => switch it with current one
    {
        if (playerUnit.Pokemon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back, {playerUnit.Pokemon.Base.Name}!");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }
        
        //send out new Pokemon
        playerUnit.Setup(newPokemon);
        dialogBox.SetMovesName(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Let's go {newPokemon.Base.Name}, it's your turn!");

        state = BattleState.RunningTurn;
        
    }
} 



