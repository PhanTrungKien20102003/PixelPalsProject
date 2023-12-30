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
    [SerializeField] bool alwaysHit;
    [SerializeField] int pp; //pp is the number of times a move can be performed
    [SerializeField] int priority; //some moves can go first even though the pokemon speed is lower 
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] List <SecondaryEffects> secondary;
    [SerializeField] MoveTarget target;


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
    public bool AlwaysHit
    {
        get { return alwaysHit; }
    }
    public int PP
    {
        get { return pp; }
    }
    public int Priority
    {
        get { return priority; }
    }
    public MoveCategory Category
    {
        get { return category; }
    }
    public MoveEffects Effects
    {
        get { return effects; }
    }

    public List <SecondaryEffects> Secondary 
    { 
        get { return secondary; } 
    }
    public MoveTarget Target
    {
        get { return target; }
    }

    /* When selecting a "move" scriptable object, I can toggle the "isSpecial" boolean to determine 
       if a move is special or not, independently of it's type */
    [SerializeField] bool isSpecial;
    public bool IsSpecial
    {
        /*get
        {
            if (type == PokemonType.Normal || type == PokemonType.Fire   || type == PokemonType.Water    || type == PokemonType.Electric ||
                type == PokemonType.Grass  || type == PokemonType.Ice    || type == PokemonType.Fighting || type == PokemonType.Poison   ||
                type == PokemonType.Ground || type == PokemonType.Flying || type == PokemonType.Psychic  || type == PokemonType.Bug      ||
                type == PokemonType.Rock   || type == PokemonType.Ghost  || type == PokemonType.Dragon   || type == PokemonType.Dark     ||
                type == PokemonType.Steel  || type == PokemonType.Fairy)
            { 
                return true; 
            }
            else
            {
                return false;
            }
        } */
        get
        {
            return isSpecial;
        }
    }
}

[System.Serializable] //show in the inspector
public class MoveEffects
{
    [SerializeField] List <StatBoost> boosts; //moves that can boost pokemon stat(s)
    [SerializeField] ConditionID status; //inflicting status condition
    [SerializeField] ConditionID volatileStatus;

    //property to expose the list of stat boost
    public List <StatBoost> Boosts
    {
        get { return boosts; }
    }
    public ConditionID Status
    {
        get { return status; }
    }
    public ConditionID VolatileStatus
    {
        get { return volatileStatus; }
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects //side effects of the move
{
    [SerializeField] int chance; //this will specify the chance for causing secondary effects like 10% burn, poison, para,...
    [SerializeField] MoveTarget target; //the second effect can affect on the user

    public int Chance
    {
        get { return chance; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}
public enum MoveCategory
{
    Physical, Special, Status
}

public enum MoveTarget
{
    Opponent, Player
}
