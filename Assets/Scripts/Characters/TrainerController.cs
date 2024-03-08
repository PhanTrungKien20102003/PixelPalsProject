using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;
    Character character;

    //state
    private bool battleLost = false;
    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFOVRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    //changing NPC direction when talking to start trainer battle
    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if (!battleLost)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                GameController.Instance.StartTrainerBattle(this);
            }));
        }
        else
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
    }
    
    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // make Trainer walk toward Player
        var diff = player.transform.position - transform.position;
        var moveVector = diff - diff.normalized;
        moveVector = new Vector3(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));
        
        yield return character.Move(moveVector);
        
        //show dialog box
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
        {
            GameController.Instance.StartTrainerBattle(this);
        }));
    }

    public void BattleLost()
    {
        battleLost = true;  
        fov.gameObject.SetActive(false);
    }
    
    public void SetFOVRotation(FacingDirection direction)
    {
        float angle = 0f;
        if (direction == FacingDirection.Right)
            angle = 90f;
        else if (direction == FacingDirection.Left)
            angle = 270f;
        else if (direction == FacingDirection.Up)
            angle = 180f;
        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;  
    }


    public object CaptureState()
    {
        return battleLost;
    }

    public void RestoreState(object state)
    {
        battleLost = (bool)state;
        
        if (battleLost == true)
            fov.gameObject.SetActive(false);

    }
}
