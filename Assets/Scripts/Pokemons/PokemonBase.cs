using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//CreateAssetMenu attribute: create new options for c# scripts
[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new Pokemon")]
public class PokemonBase : ScriptableObject
{
    /* I like to use SerializeField attribute instead of making my variables public
       because we need to use this variable outside this class
       so I expose these variables by using functions or properties */
    [SerializeField] string name;

    [TextArea] //to have some space to type in the description
    [SerializeField] string description; //variable for description

    //variables for Front and Back sprites in the battle system
    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    /* variables for Pokemon Types 
       and I use Enum so I'll have a nice picker in the UI */
    [SerializeField] PokemonType type1;
    [SerializeField] PokemonType type2;

    //variables for Base Stats
    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    
    [SerializeField] int expYield; //experience of each Pokemon
    [SerializeField] GrowthRate growthRate; //growth rate of each Pokemon to see which will level up faster

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves; /* I prefer using list than array although they almost like the same
                                                            but it comes with some more predefined functions which will be useful */

    [SerializeField] List<Evolution> evolutions;
    public static int MaxNumOfMoves { get; set; } = 4;
    public int GetExpForLevel(int level)
    {
        if (growthRate == GrowthRate.Fast)
        {
            return Mathf.FloorToInt(4 * (level * level * level) / 5f);
        }    
        else if (growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }
        else if (growthRate == GrowthRate.MediumSlow)
        {
            return 6 * (level * level * level) / 5 - 15 * (level * level) + 100 * level - 140;
        }
        else if (growthRate == GrowthRate.Slow)
        {
            return 5 * (level * level * level) / 4;
        }
        else if (growthRate == GrowthRate.Fluctuating)
        {
            return GetFluctuating(level);
        }
        return -1;
    }
    public int GetFluctuating(int level)
    {
        if (level <= 15)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor((level + 1) / 3) + 24) / 50));
        }
        else if (level >= 15 && level <= 36)
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((14 + level) / 50));
        }
        else
        {
            return Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(level / 2) + 32) / 50));
        }
    }
    
    //Properties
    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }
    public Sprite BackSprite
    {
        get { return backSprite; }
    }
    public PokemonType Type1
    {
        get { return type1; }
    }
    public PokemonType Type2
    {
        get { return type2; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }
    public int Attack
    {
        get { return attack; }
    }
    public int Defense
    {
        get { return defense; }
    }
    public int SpAttack
    {
        get { return spAttack; }
    }
    public int SpDefense
    {
        get { return spDefense; }
    }
    public int Speed
    {
        get { return speed; }
    }
    public List<LearnableMove> LearnableMoves 
    {
        get { return learnableMoves; }
    }
    
    public List<Evolution> Evolutions => evolutions;
    public int CatchRate => catchRate;
    
    public int ExpYield => expYield;
    public GrowthRate GrowthRatio => growthRate;
    

    [System.Serializable]
    public class LearnableMove //store list of moves the pokemon can learn and the level at which it will learn them all
    {
        [SerializeField] MoveBase moveBase; //reference to move base

        [SerializeField] int level; //level at which move will be learned

        //properties to expose them
        public MoveBase Base
        {
            get { return moveBase; }
        }
        public int Level
        {
            get { return level; }
        }
    }

    [System.Serializable]
    public class Evolution
    {
        [SerializeField] PokemonBase evolvesInto;
        [SerializeField] int requiredLevel;

        public PokemonBase EvolvesInto => evolvesInto;
        public int RequiredLevel => requiredLevel;
    }

    public enum PokemonType /* Enum represents different types of Pok�mon. Each Pok�mon type is assigned a distinct constant value, 
                               starting from 0 for the first value and incrementing sequentially 
                               It makes the code more readable and reducing the risk of errors when working with related values */
    {
        None,
        Normal,
        Fire,
        Water,
        Electric,
        Grass,
        Ice,
        Fighting,
        Poison,
        Ground,
        Flying,
        Psychic,
        Bug,
        Rock,
        Ghost,
        Dragon,
        Dark,
        Steel,
        Fairy
    }

    public enum GrowthRate //enum for how fast Pokemon can level up base on the growth rate
    {
        Fast,
        MediumFast,
        MediumSlow,
        Slow,
        Fluctuating
    }

    public enum Stat
    {
        Attack,
        Defense,
        SpAttack,
        SpDefense,
        Speed,

        //these are not 2 actual stats, they're used to boost the moveAccuracy
        Accuracy,
        Evasion
    }

    public class TypeChart
    {
        static float[][] chart = //use static to use directly from the class without creating an object
        {
        //Has to be same order as PokemonType class

        //                       Nor   Fir   Wat   Ele   Gra   Ice   Fig   Poi   Gro   Fly   Psy   Bug   Roc   Gho   Dra   Dar  Ste    Fai
        /*Normal*/   new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 0,    1f,   1f,   0.5f, 1f},
        /*Fire*/     new float[] {1f,   0.5f, 0.5f, 1f,   2f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 1f,   2f,   1f},
        /*Water*/    new float[] {1f,   2f,   0.5f, 1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   1f,   1f},
        /*Electric*/ new float[] {1f,   1f,   2f,   0.5f, 0.5f, 1f,   1f,   1f,   0f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f},
        /*Grass*/    new float[] {1f,   0.5f, 2f,   1f,   0.5f, 1f,   1f,   0.5f, 2f,   0.5f, 1f,   0.5f, 2f,   1f,   0.5f, 1f,   0.5f, 1f},
        /*Ice*/      new float[] {1f,   0.5f, 0.5f, 1f,   2f,   0.5f, 1f,   1f,   2f,   2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f},
        /*Fighting*/ new float[] {2f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f, 0.5f, 0.5f, 2f,   0f,   1f,   2f,   2f,   0.5f},
        /*Poison*/   new float[] {1f,   1f,   1f,   1f,   2f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   1f,   0.5f, 0.5f, 1f,   1f,   0f,   2f},
        /*Ground*/   new float[] {1f,   2f,   1f,   2f,   0.5f, 1f,   1f,   2f,   1f,   0f,   1f,   0.5f, 2f,   1f,   1f,   1f,   2f,   1f},
        /*Flying*/   new float[] {1f,   1f,   1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 1f},
        /*Psychic*/  new float[] {1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   1f,   1f,   0.5f, 1f,   1f,   1f,   1f,   0f,   0.5f, 1f},
        /*Bug*/      new float[] {1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 0.5f, 1f,   0.5f, 2f,   1f,   1f,   0.5f, 1f,   2f,   0.5f, 0.5f},
        /*Rock*/     new float[] {1f,   2f,   1f,   1f,   1f,   2f,   0.5f, 1f,   0.5f, 2f,   1f,   2f,   1f,   1f,   1f,   1f,   0.5f, 1f},
        /*Ghost*/    new float[] {0f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   2f,   1f,   0.5f, 1f,   1f},
        /*Dragon*/   new float[] {1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   1f,   2f,   1f,   0.5f, 0f},
        /*Dark*/     new float[] {1f,   1f,   1f,   1f,   1f,   1f,   0.5f, 1f,   1f,   1f,   2f,   1f,   1f,   2f,   1f,   0.5f, 1f,   0.5f},
        /*Steel*/    new float[] {1f,   0.5f, 0.5f, 0.5f, 1f,   2f,   1f,   1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   0.5f, 2f},
        /*Fairy*/    new float[] {1f,   0.5f, 1f,   1f,   1f,   1f,   2f,   0.5f, 1f,   1f,   1f,   1f,   1f,   1f,   2f,   2f,   0.5f, 1f}
        };
        public static float GetEffectiveness(PokemonType attackType, PokemonType defenseType)
        {
            if (attackType == PokemonType.None || defenseType == PokemonType.None)
                return 1;

            // substract 1 from the integer value of the enum because the index of the "Normal" TypeChart class is 0
            int row = (int)attackType - 1; 
            int col = (int)defenseType - 1;

            return chart[row][col];
        }
    }
}

