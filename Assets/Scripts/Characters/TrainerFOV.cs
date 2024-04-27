using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFOV : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OneEnterTrainersView(GetComponentInParent<TrainerController>());
    }
    public bool TriggerRepeatedly => false;
}
