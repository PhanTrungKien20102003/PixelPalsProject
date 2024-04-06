using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

//variable to store the state of the battle
public enum BattleState {Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartySlot, AboutToUse, MoveToForget, BattleOver}
public enum BattleAction { Move, SwitchPokemon, UseItem, Run}
public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartySlot partySlot;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject pokeballSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;
    public event Action <bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove; /*if current action is 0 -> "Fight" will be selected
                                            1 -> "Run" will be selected */
    private bool aboutToUseChoice = true;
    
    //show in private variables
    PokemonParty playerParty; 
    PokemonParty trainerParty;
    Pokemon wildPokemon;

    bool isTrainerBattle = false;// indicate if this is a trainer battle or not
    PlayerController player;
    TrainerController trainer;

    int runAttempts;
    MoveBase moveToLearn;
    
    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon) /*pass the player's party and the wild pokemon
                                                                                  while calling the StartBattle() function */
    {
        //assign parameters to variables
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        player = this.playerParty.GetComponent<PlayerController>();
        isTrainerBattle = false;

        StartCoroutine(SetupBattle());
    }
    
    public void StartTrainerBattle(PokemonParty playerParty, PokemonParty trainerParty)
        {
            //assign parameters to variables
            this.playerParty = playerParty;
            this.trainerParty = trainerParty;
            
            isTrainerBattle = true;
            player = this.playerParty.GetComponent<PlayerController>();
            trainer = this.trainerParty.GetComponent<TrainerController>();
            
            StartCoroutine(SetupBattle());
        }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();
        
        if (!isTrainerBattle) 
        {
            //Wild Pokemon Battle
            playerUnit.Setup(playerParty.GetHealthyPokemon()); //create player's Pokemon that aren't fainted
            enemyUnit.Setup(wildPokemon);  //create opponents's Pokemon
            
            dialogBox.SetMovesName(playerUnit.Pokemon.Moves); //passing the moves of Pokemon to the SetMovesName function

            /* use yield return statement to execute another coroutine -> this will wait the "dialogBox.TypeDialog(...)" coroutine to be completed
             and only after that, the execution will come down */
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared!");
        }
        else
        {
            //Trainer Battle
            
            //Show trainer and player sprite
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;
            
            //show dialog saying this trainer wants to battle
            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");
            
            //send out first pokemon of the trainer
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyPokemon = trainerParty.GetHealthyPokemon();
            enemyUnit.Setup(enemyPokemon);
            yield return dialogBox.TypeDialog($"{trainer.Name} send out {enemyPokemon.Base.Name}!");
            
            //send out first pokemon of the player
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerPokemon = playerParty.GetHealthyPokemon();
            playerUnit.Setup(playerPokemon);
            yield return dialogBox.TypeDialog($"Go {playerPokemon.Base.Name}!");
            dialogBox.SetMovesName(playerUnit.Pokemon.Moves); //passing the moves of Pokemon to the SetMovesName function

        }

        runAttempts = 0;
        partySlot.Init();
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

    void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);    
    }
    
    void OpenPartySlot()
    {
        partySlot.CalledFrom = state;
        state = BattleState.PartySlot;
        partySlot.gameObject.SetActive(true);
    }
    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator AboutToUse(Pokemon newPokemon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog(
            $"{trainer.Name} is about to use {newPokemon.Base.Name}. Do you want to switch Pokemon?");
        
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Pokemon pokemon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget!");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(pokemon.Moves.Select(x => x.Base).ToList(), newMove);
                                                //using Link => convert a list of Move.cs class into a list of MoveBase.cs class
        moveToLearn = newMove;
                                                
        state = BattleState.MoveToForget;
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
                var selectedPokemon = partySlot.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchPokemon(selectedPokemon);
            }
            else if (playerAction == BattleAction.UseItem)
            {
                // This is handled from item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
            }
            else if (playerAction == BattleAction.Run)
            {
                yield return TryToRun();
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
            yield return sourceUnit.HUD.WaitForHPUpdate();
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
                yield return targetUnit.HUD.WaitForHPUpdate();
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
                yield return HandlePokemonFainted(targetUnit);
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
        yield return sourceUnit.HUD.WaitForHPUpdate();

        if (sourceUnit.Pokemon.HP <= 0)
        {
            yield return HandlePokemonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);

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

    IEnumerator HandlePokemonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Pokemon.Base.Name} fainted!");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            //gain exp
            int expYield = faintedUnit.Pokemon.Base.ExpYield;
            int enemyLevel = faintedUnit.Pokemon.Level;

            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f; //if the battle is the trainer battle, gain more exp
            
            int expGain = Mathf.FloorToInt(expYield * enemyLevel * trainerBonus) / 7; //formula to calculate the exp gain
            playerUnit.Pokemon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} gained {expGain} exp!");
            yield return playerUnit.HUD.SetExpSmoothly();
            
            //check if player unit has gained enough exp to lvl up
            while (playerUnit.Pokemon.CheckForLevelUp())
            {
                playerUnit.HUD.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} grew to level {playerUnit.Pokemon.Level}!");
                
                //try to learn a new move
                var newMove = playerUnit.Pokemon.GetLearnableMoveAtCurrentLevel();
                if (newMove != null)
                {
                    if (playerUnit.Pokemon.Moves.Count < PokemonBase.MaxNumOfMoves)
                    {
                        playerUnit.Pokemon.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} learned {newMove.Base.Name}!");
                        dialogBox.SetMovesName(playerUnit.Pokemon.Moves);
                    }
                    else
                    {
                        //Option to forget a move
                        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} trying to learn {newMove.Base.Name}!");
                        yield return dialogBox.TypeDialog($"But it can not learn more than {PokemonBase.MaxNumOfMoves} moves!");
                        yield return ChooseMoveToForget(playerUnit.Pokemon, newMove.Base);
                        yield return new WaitUntil(() => state != BattleState.MoveToForget);
                        yield return new WaitForSeconds(2f);

                    }
                }
                
                yield return playerUnit.HUD.SetExpSmoothly(true);
            }

            yield return new WaitForSeconds(1f);
        }
        CheckForBattleOver(faintedUnit);
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
            if (!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextPokemon = trainerParty.GetHealthyPokemon();
                if (nextPokemon != null)
                {
                    StartCoroutine(AboutToUse(nextPokemon));
                }
                else
                    BattleOver(true);
            }
        }
    }

    //function to show the damage details in the dialog box
    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
         yield return dialogBox.TypeDialog("A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if (damageDetails.TypeEffectiveness is < 1f and > 0f)
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
        else if (state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action onItemUsed = () =>
            {
                state = BattleState.Busy;
                inventoryUI.gameObject.SetActive(false);
                StartCoroutine(RunTurns(BattleAction.UseItem));
            };
            
            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if (state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if (state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if (moveIndex == PokemonBase.MaxNumOfMoves)
                {
                    // Don't learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} did not learn {moveToLearn.Name}!"));
                    
                }
                else
                {
                    //Forget the selected move and learn the new move
                    var selectedMove = playerUnit.Pokemon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}!"));
                    
                    playerUnit.Pokemon.Moves[moveIndex] = new Move(moveToLearn);
                }
                moveToLearn = null;
                state = BattleState.RunningTurn;
            };
            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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
                OpenBag();
            }
            if (currentAction == 2)
            {
                //Pokemon
                OpenPartySlot();
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
            }
        }
    }
    void HandleMoveSelection() //responsible for selecting moves
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            currentMove++;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            currentMove--;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

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
        Action onSelected = () =>
        {
            var selectedMember = partySlot.SelectedMember;
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

            if (partySlot.CalledFrom == BattleState.ActionSelection) //if the player choose to switch pokemon during a turn => calling run turns
            {
                StartCoroutine(RunTurns(BattleAction.SwitchPokemon));
            }
            else //if the player choose switch pokemon because the current one fainted => directly call SwitchPokemon function
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partySlot.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchPokemon(selectedMember, isTrainerAboutToUse));
            }
            
            partySlot.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if (playerUnit.Pokemon.HP <= 0)
            {   
                partySlot.SetMessageText("You have to choose a Pokemon to continue!");
                return;
            }
            
            partySlot.gameObject.SetActive(false);

            if (partySlot.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerPokemon());
            }
            else
                ActionSelection();
            
            partySlot.CalledFrom = null;
        };
            
        partySlot.HandleUpdate(onSelected, onBack);
    }

    void HandleAboutToUse() //responsible for selecting pokemon
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))  
            aboutToUseChoice = !aboutToUseChoice;
        
        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice == true)
            {
                // Yes option
                OpenPartySlot();
            }
            else
            {
                // No option
                StartCoroutine(SendNextTrainerPokemon());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerPokemon());
        }
    }
    
    IEnumerator SwitchPokemon(Pokemon newPokemon, bool isTrainerAboutToUse = false) //if the player actually selected a valid pokemon => switch it with current one
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

        if (isTrainerAboutToUse)
            StartCoroutine(SendNextTrainerPokemon()); 
        else
            state = BattleState.RunningTurn;
    }

    IEnumerator SendNextTrainerPokemon()
    {
        state = BattleState.Busy;

        var nextPokemon = trainerParty.GetHealthyPokemon();
        enemyUnit.Setup(nextPokemon);
        yield return dialogBox.TypeDialog($"{trainer.Name} send out {nextPokemon.Base.Name}!");
        
        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowPokeball()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't catch other Pokemon!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} use a Pokeball!");

        var pokeballObject = Instantiate(pokeballSprite, playerUnit.transform.position - new Vector3(2,0), Quaternion.identity);
        var pokeball = pokeballObject.GetComponent<SpriteRenderer>();
        
        //Animations for pokeball
        yield return pokeball.transform.DOJump(enemyUnit.transform.position + new Vector3(0,2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return pokeball.transform.DOMoveY(enemyUnit.transform.position.y - 1f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchPokemon(enemyUnit.Pokemon);
        //Shaking pokeball animation
        for (int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        { 
            yield return new WaitForSeconds(0.75f);
;           pokeball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion(); 
        }
        if (shakeCount == 4) 
        {
            //Pokemon is caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} was caught!");
            yield return pokeball.DOFade(0, 1.5f).WaitForCompletion();
            
            playerParty.AddPokemon(enemyUnit.Pokemon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} has been added to your party!");
            
            Destroy(pokeball);
            BattleOver(true);
        }
        else
        {
            //Pokemon broke out
            yield return new WaitForSeconds(1f);
            pokeball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakOutAnimation();
            
            if (shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} broke free!");
            else
                yield return dialogBox.TypeDialog($"Almost caught it!");
            
            Destroy(pokeball);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchPokemon(Pokemon pokemon)
    {
        float a = (3 * pokemon.MaxHp - 2 * pokemon.HP) * pokemon.Base.CatchRate * ConditionsDB.GetStatusBonus(pokemon.Status) / (3 * pokemon.MaxHp);

        if (a >= 255)
            return 4;

        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while (shakeCount < 4)
        {
            if (UnityEngine.Random.Range(0, 65535) >= b)
                break;
            shakeCount++;
        }
        return shakeCount;
    }

    IEnumerator TryToRun()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battle!");
            state = BattleState.RunningTurn;
            yield break;
        }
        runAttempts++;
        
        int playerSpeed = playerUnit.Pokemon.Speed;
        int enemySpeed = enemyUnit.Pokemon.Speed;

        if (enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"You have run away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * runAttempts;
            f = f % 256;

            if (UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"You have run away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"You can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
} 



