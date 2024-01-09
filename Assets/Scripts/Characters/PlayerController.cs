using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public event Action OnEncountered; //import "System" namespace

    private Vector2 input; //move the player

    private Character character; //reference to Character script
    private void Awake() //add animator
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate() /*change the name from Update() to HandleUpdate() so it won't be call automatically by Unity
                                 and can call this from inside the game Controller*/
    {
        if (!character.IsMoving) //variable to keep track if whether the player is moving or not
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            //using GetAxisRaw, the input will always be 1 or -1
            //press right key -> input 1; left key -> input -1


            //remove diagonal movement
            if (input.x != 0) input.y = 0; //only one of these can be non-zero at a time


            if (input != Vector2.zero) //if input isn't 0, set moveX and moveY parameter of animation
            {
                StartCoroutine(character.Move(input, CheckForEncounters));
            }
        }
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
            Interact();
    }

    void Interact() //talk with NPCs
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY); //find the tile where player is facing and check if there is any interactable object in that tile
        var interactPos = transform.position + facingDir; //postion of the tile to which the player is facing

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractableLayer); //check if there any interactable object in interactPos

        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

/* - How to tell the GameController that I need to go to the battleState? 
     => Grab the reference to the GameController and PlayerController and just called a function.

   - But the problem is I'm already have a reference of the PlayerController inside the GameController
     => If I create a reference back to it, this will create a circular dependency which can cause lots of problems
        and that is sth I want to avoid.
    
   - Instead, I will use the Observer Desgin Pattern to solve this

   -----------------------------------------------------------
   |                                                         |
   | GameController ---------------------> PlayerController  |
   |   (Observer)                             (Subject)      |
   |                                                         |
   -----------------------------------------------------------

    With observer pattern we'll be able to inverse the dependency. 
    So the PlayerController doesn't have to reference the GameController. 

     => Inside the PlayerController, I will create an event and the GameController will subscribe to the event and whenever
        there's an encounter, we'll just invoke the event and any objects that are subscribed that event will be notified 
        that it happened.
 */

    private void CheckForEncounters() //this function will handle the battle triggering logic
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.GrassLayer) != null) //check if the tile when the player move into is actually a grass tile 
                                   //position of player       //null means the player step on a grass tile
        
        {
            if (UnityEngine.Random.Range(1,101) <=10)
            {
                character.Animator.IsMoving = false; //once the battle start, the animation will stop
                OnEncountered();
            }
        }
    }

}
