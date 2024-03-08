using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Pool;
using static PokemonBase;
using Random = UnityEngine.Random;

[System.Serializable] //classes will only be shown in the Inspector if I use this attribute
public class Pokemon
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        _base = pBase;
        level = pLevel;
        
        Init();
    }
    
    public PokemonBase Base 
    {
        get
        {
            return _base;   
        }
    }
    public int Level 
    {
        get
        {
            return level;
        }
    }

    public int Exp {get; set; }
    public int HP { get; set; } //variable to store the current HP so the HP can reduce during the battle and be available to save that

    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }


    //do stats increase/decrease once and store the values of the stats at the current level 
    public Dictionary<Stat, int> Stats { get; private set; }
                     //key                    //set private to only calculate and set inside the PokemonLevel class

    public Dictionary<Stat, int> StatBoost { get; private set; }

    public Conditions Status { get; private set; } //store status conditions of the pokemon

    public int StatusTime {  get; set; } //specify how many turns the pokemon should sleep

    public Conditions VolatileStatus { get; private set; } //means the side effect status such as confusion and hurt itself
    public int VolatileStatusTime { get; set; }

    /* use to store a list of element same as the list but the difference is 
       you can take out elements from the queue in the order in which add it 
       to the queue */
    public Queue <string> StatusChanges { get; private set; }

    public bool HPChanged { get; set; }

    public event System.Action OnStatusChanged;

    public void Init()                     //creating objects of PokemonLevel class from the Inspector itself
               //stand for initalization
    {
        //generate moves of Pokemon base on it level
        Moves = new List<Move>();
        foreach(var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= PokemonBase.MaxNumOfMoves)
                break;
        }
        
        Exp = Base.GetExpForLevel(Level);
        
        CalculateStats();
        HP = MaxHp;

        StatusChanges = new Queue<string>();
        ResetStatusBoost();
        Status = null;
        VolatileStatus = null;
    }

    void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((2 * Base.MaxHp * Level) / 100f) + Level + 10;
    }

    void ResetStatusBoost() //reset boost when battle is over
    {
        StatBoost = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    //properties to calculate the stats of Pokemon at the current level
    //using FloorToInt function to remove decimal point
    //I'm using the actual formula used in the game
    public int MaxHp
    {
        get; private set;
    }

    int GetStat(Stat stat)
    {
        int statValue = Stats[stat];

        //apply stat boost
        int boost = StatBoost[stat];

        /* If the value of boost is 1, means that the stat is increased by one level, in that case it'll multiply the stat value with 1.5
           Maximun increase/decrease is 6 times 
           Similarly with the stat that is decreased too */
        var boostValue = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f };

        if (boost >= 0)
        {
            statValue = Mathf.FloorToInt(statValue * boostValue[boost]);
        }
        else
        {
            statValue = Mathf.FloorToInt(statValue / boostValue[-boost]); //when negative, can't pass the boost directly as the index => negate it
        }
        return statValue;
    }

    //modify stat boost dictionary when a status move is perform
    public void ApplyBoosts(List <StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts) //loop through all the elements in the list
        {
            //store value into variables
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            //add boost value to the current value of the stat in stat boost dictionary
            StatBoost[stat] = Mathf.Clamp(StatBoost[stat] + boost, -6, 6);

            if (boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }

            Debug.Log($"{stat} boosted to {StatBoost[stat]}");
        } 
    }

    public bool CheckForLevelUp()
    {
        if (Exp > Base.GetExpForLevel(level + 1))
        {
            level++;
            return true;
        }
        return false;
    }

    public LearnableMove GetLearnableMoveAtCurrentLevel()
    {
        return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    }

    public void LearnMove(LearnableMove moveToLearn)
    {
        if (Moves.Count > PokemonBase.MaxNumOfMoves)
            return;
        Moves.Add(new Move(moveToLearn.Base));    
    }
    
    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }
    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }
    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }
    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }
    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }
    public DamageDetails TakeDamage(Move move, Pokemon attacker) /* formula to calculate the damage 
                                                                       use DamageDetails function to return the damage deatils
                                                                       instead of a boolean */
    {
        //the chance for a move to land a critical hit is 6.25% and it will double the damage of the move
        float critical = 1f;
        if (Random.value * 100f <= 6.25f)
            critical = 2f;

        // this will calculate if the move is super effective or not very effective base on the type
        float type = PokemonBase.TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * PokemonBase.TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = type,
            Critical = critical,
            Fainted = false
        };

        //calculate the damage if the Pokemon was hit by Attack move or Sp.Attack move
        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) attack / defense);
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    public void UpdateHP(int damage) //use this function whenever want to reduce HP
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        HPChanged = true;
    }

    public void SetStatus(ConditionID condition_id)
    {
        if (Status != null) //prevent setting status if pokemon already has another status
        {
            return;
        }

        Status = ConditionsDB.Conditions[condition_id];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");

        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID condition_id)
    {
        if (VolatileStatus != null)
        {
            return;
        }

        VolatileStatus = ConditionsDB.Conditions[condition_id];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        //prevent enemy from performing a move that doesn't have PP
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if (Status?.OnBeforeMove != null)
        {
            if (!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if (VolatileStatus?.OnBeforeMove != null)
        {
            if (!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }
        return canPerformMove;
    }
    public void OnAfterTurn() //call this function once the pokemon's turn is over
    {
        /*Apply the null conditional operator on status so that if the pokemon doesn't have any status => not get null reference error*/
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatusBoost();
    }
}

//show the player that they can know whether it was a super effective or not effective and a critical hit has landed in the battle dialog 
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
