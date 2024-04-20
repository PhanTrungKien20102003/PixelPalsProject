using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    
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
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }
        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Space))
            StartCoroutine(Interact());
    }

    IEnumerator Interact() //talk with NPCs
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY); //find the tile where player is facing and check if there is any interactable object in that tile
        var interactPos = transform.position + facingDir; //postion of the tile to which the player is facing

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractableLayer); //check if there any interactable object in interactPos

        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
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

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.Instance.TriggerableLayers);

        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }
    
    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;  
    }
    public Character Character => character;
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList()
        };
        return saveData;
        
        
        /*the position is the Vector 3 class -> while using a saving system, can only use classes that are serializable.
          Only then, the saving system will know how to convert that class into binary.
          So I have to convert vector 3 into some other form which I can save */
        float[] position = new float[] { transform.position.x, transform.position.y }; 
        return position;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        
        //Restore the position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);
        
        //Restore Party
        GetComponent<PokemonParty>().Pokemons =  saveData.pokemons.Select(s => new Pokemon(s)).ToList();
    }
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}
