using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

//contains all common behaviors 
public class Character : MonoBehaviour
{
    public float moveSpeed;             //variable for the movement speed

    public bool IsMoving { get; private set; }

    CharactersAnimator animator;        //use custom animator, not using Unity's animator system
    private void Awake()
    {
        animator = GetComponent<CharactersAnimator>();
    }
    public IEnumerator Move(Vector3 moveVector, Action OnMoveOver = null ) //moveVector to calculate target postion
                                                      //not compulsory while calling this function
    {
        //setting parameters of the animator
        /*In the case of NPCs, the value can be much larger, might want the NPC to move 4-5 tiles 
          => use Clamp between -1 and 1*/
        animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f); /* set moveX and moveY and if not 0, the player will stay in the previous animation
                                                        when not moving */
        animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);

        //calculate the target position
        var targetPos = transform.position; //current position of the player or character plus the "input"
        targetPos.x += moveVector.x;
        targetPos.y += moveVector.y;

        //if target position is a walkable tile and don't have objects next to -> execute moving code below
        if(!IsPathClear(targetPos))
            yield break;

        //moving character to target position 
        IsMoving = true;
            
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon) /* check if the difference between the target postion 
                                                                              and the target postion is greater than the value */

        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; //stop the execution of the coding and resume it in the next Update funcion

            /* if there is actually a difference between both positions then use MoveTowards function 
             to move the player towards the target postion by a very small amount */
        }
        transform.position = targetPos;

        IsMoving = false;

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsPathClear(Vector3 targetPos)
    {
        var diff = targetPos - transform.position;
        var dir = diff.normalized; //return another vector with same direction as "diff" but the length will be 1

        if (Physics2D.BoxCast(transform.position + dir, new Vector2(0.2f, 0.2f), 0f, dir, diff.magnitude - 1, 
            GameLayers.Instance.SolidLayer | GameLayers.Instance.InteractableLayer | GameLayers.Instance.PlayerLayer) == true)
        {
            return false;
        }
        return true;
    }

    private bool IsWalkable(Vector3 targetPos) //take the target position and check if that tile at that position is walkable
    {
        /* first parameter is target postion
           second parameter is the radius of the circle need to check
           third parameter is to pass the layer of the object that we want to check */
        if (Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.SolidLayer | GameLayers.Instance.InteractableLayer) != null)  
                                                                                      //not null means there is actually a solid object in target position    
        { // => not walkable so return to false
            return false;
        }
        return true;
    }

    //NPCs will face toward the Player when talking
    public void LookTowards(Vector3 targetPos)
    {
        //find the different between x and y coordinates of the position of the character and the target position
        var xDiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x); //store the different in the x coordinate
        var yDiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y); //store the different in the y coordinate

        if (xDiff == 0 || yDiff == 0)
        {
            animator.MoveX = Mathf.Clamp(xDiff, -1f, 1f); 
            animator.MoveY = Mathf.Clamp(yDiff, -1f, 1f);
        }
        else
            Debug.Log("Error in Look Towards: You can't ask the character to look diagonally!");
    }
    public CharactersAnimator Animator {
        get => animator;
    }
}
