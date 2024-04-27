using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;
    
    [SerializeField] Dialog startDialog;
    [SerializeField] Dialog inProgressDialog;
    [SerializeField] Dialog completeDialog;
    
    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;

    public string Name => name;
    public string Description => description;
    
    public Dialog StartDialog => startDialog;
    public Dialog InProgressDialog => inProgressDialog?.Lines?.Count > 0 ? inProgressDialog : startDialog;
    public Dialog CompleteDialog => completeDialog;
    
    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
