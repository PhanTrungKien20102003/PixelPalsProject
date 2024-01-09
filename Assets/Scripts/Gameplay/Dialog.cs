using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
//make it a class for NPCs to give an item the the user after the dialog or trigger story item/quest
public class Dialog
{
    [SerializeField] List<string> lines;
    public List<string> Lines { get { return lines; } }
}
