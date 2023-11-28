using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PokemonBase;

[CreateAssetMenu(fileName = "Move", menuName ="Pokemon/Create a new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] PokemonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp; //pp is the number of times a move can be performed


    //properties to expose variables
    public string Name
    {
        get { return name; }
    }
    public string Description
    {
        get { return description; }
    }
    public PokemonType Type
    {
        get { return type; }
    }
    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }


    /* When selecting a "move" scriptable object, I can toggle the "isSpecial" boolean to determine 
       if a move is special or not, independently of it's type */
    [SerializeField] bool isSpecial;
    public bool IsSpecial
    {
        get
        {
            return isSpecial;
        }
    }
}
