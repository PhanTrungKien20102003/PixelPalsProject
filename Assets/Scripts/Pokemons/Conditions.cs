using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conditions 
{
    //properties
    public ConditionID id {  get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string StartMessage { get; set; } //message shown when a pokemon is inflicted with the status

    public Action <Pokemon> OnStart { get; set; }

    public Func <Pokemon, bool> OnBeforeMove { get; set; } //for status like paralyze, frozen and sleep that might prevent a pokemon from performing a move
    public Action <Pokemon> OnAfterTurn { get; set; } //reduce pokemon HP after each turn
}
