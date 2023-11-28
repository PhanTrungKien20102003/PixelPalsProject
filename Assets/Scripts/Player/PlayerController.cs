using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;             //variable for the movement speed

    public LayerMask solidObjectsLayer; //variable for the solid object that can't go through

    public LayerMask grassLayer;        //variable for grass layer to get Encounter battle

    public event Action OnEncountered; //import "System" namespace

    private bool isMoving; //variable to check if the player is moving or not

    private Vector2 input; //move the player

    private Animator animator; //add animation to animator controller
    private void Awake() //add animator
    {
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate() /*change the name from Update() to HandleUpdate() so it won't be call automatically by Unity
                                 and can call this from inside the game Controller*/
    {
        if (!isMoving) //variable to keep track if whether the player is moving or not
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
            //using GetAxisRaw, the input will always be 1 or -1
            //press right key -> input 1; left key -> input -1


            //remove diagonal movement
            if (input.x != 0) input.y = 0; //only one of these can be non-zero at a time


            if (input != Vector2.zero) //if input isn't 0, set moveX and moveY parameter of animation
            {
                animator.SetFloat("moveX", input.x); /* set moveX and moveY and if not 0, the player will stay in the previous animation
                                                        when we're not moving */ 
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position; //current position of the player pluse the "input"
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos)) 
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving); // set variable !isMoving to the animator at the end of Update function
    }
    
    IEnumerator Move(Vector3 targetPos)
    //moving the player from current position to the target position
    //used to do sth over a period of time
    {
        isMoving = true;

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon) /* check if the difference between the target postion 
                                                                              and the target postion is greater than the value */                        
        
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; //stop the execution of the coding and resume it in the next Update funcion

            /* if there is actually a difference between both positions then use MoveTowards function 
             to move the player towards the target postion by a very small amount */
        }
        transform.position = targetPos;

        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos) //take the target position and check if that tile at that position is walkable
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)  /* first parameter is target postion
        //not null means there is actually a solid object in target position        second parameter is the radius of the circle need to check
                                                                                    third parameter is to pass the layer of the object that we want to check */
        { // => not walkable so return to false
            return false;
        }
        return true;
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
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null) //check if the tile when the player move into is actually a grass tile 
                                   //position of player                            //null means the player step on a grass tile
        
        {
            if (UnityEngine.Random.Range(1,101) <=10)
            {
                animator.SetBool("isMoving", false); //once the battle start, the animation will stop
                OnEncountered();
            }
        }
    }

}
