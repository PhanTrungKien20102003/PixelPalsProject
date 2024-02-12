using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*this class is for control different animation states without using Unity's Animation system*/
public class CharactersAnimator : MonoBehaviour
{
    //frames to pass animations
    [SerializeField] List<Sprite> walkDownSprites;
    [SerializeField] List<Sprite> walkUpSprites;
    [SerializeField] List<Sprite> walkLeftSprites;
    [SerializeField] List<Sprite> walkRightSprites;
    [SerializeField] private FacingDirection defaultDirection = FacingDirection.Down;


    //use same parameters which used in animator controller    
    public float MoveX {  get; set; }
    public float MoveY {  get; set; }
    public bool IsMoving {  get; set; }

    //states here going to be different animations
    SpriteAnimator walkDownAnim;
    SpriteAnimator walkUpAnim;
    SpriteAnimator walkLeftAnim;
    SpriteAnimator walkRightAnim;

    SpriteAnimator currentAnim; //variable to store current animation
    bool wasPreviouslyMoving;

    // references
    SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        //initialize all animations
        walkDownAnim = new SpriteAnimator(walkDownSprites, spriteRenderer);
        walkUpAnim = new SpriteAnimator(walkUpSprites, spriteRenderer);
        walkLeftAnim = new SpriteAnimator(walkLeftSprites, spriteRenderer);
        walkRightAnim = new SpriteAnimator(walkRightSprites, spriteRenderer);
        
        SetFacingDirection(defaultDirection);
        currentAnim = walkDownAnim;
    }

    private void Update() //decide which animation to play based on the value of MoveX and MoveY
    {
        /* if the current animation was changed during the Update -> call currentAnim.Start() 
           => Reset everything in SpriteAnimator class Start() function */
        //store the currentAnim
        var preAnim = currentAnim; //check if the current animation has changed or not

        if (MoveX == 1)
            currentAnim = walkRightAnim;
        else if (MoveX == -1)
            currentAnim = walkLeftAnim;
        else if (MoveY == 1)
            currentAnim = walkUpAnim;
        else if (MoveY == -1)
            currentAnim = walkDownAnim;
                   
        //if the current animation not equal to previous animation -> the current animation has changed
        if (currentAnim != preAnim || IsMoving != wasPreviouslyMoving) 
            currentAnim.Start();

        /*play the animation if IsMoving is true, otherwise just show the first frame of the animation */
        if (IsMoving)
            currentAnim.HandleUpdate();
        else 
            spriteRenderer.sprite = currentAnim.Frames[0];

        wasPreviouslyMoving = IsMoving;
    }

    public void SetFacingDirection(FacingDirection direction)
    {
        if (direction == FacingDirection.Right)
            MoveX = 1;
        else if (direction == FacingDirection.Left)
            MoveX = -1;
        else if (direction == FacingDirection.Down)
            MoveY = -1;
        else if (direction == FacingDirection.Up)
            MoveY = 1;
    }

    public FacingDirection DefaultDirection
    {
        get => defaultDirection;
    }
}

public enum FacingDirection {Up, Down, Left, Right}

