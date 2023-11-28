using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable] //classes will only be shown in the Inspector if I use this attribute
public class PokemonLevel
{
    [SerializeField] PokemonBase _base;
    [SerializeField] int level;
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

    public int HP { get; set; } //variable to store the current HP so the HP can reduce during the battle and be available to save that

    public List<Move> Moves { get; set; }

    public void Init()                      //creating objects of PokemonLevel class from the Inspector itself
               //stand for initalization
    {
        HP = MaxHp;

        //generate moves of Pokemon base on it level
        Moves = new List<Move>();
        foreach(var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.Base));

            if (Moves.Count >= 4)
                break;
        }

    }

    //properties to calculate the stats of Pokemon at the current level
    //using FloorToInt function to remove decimal point
    //I'm using the actual formula used in the game
    public int MaxHp
    {
        get { return Mathf.FloorToInt((2 * Base.MaxHp * Level) / 100f) + Level + 10; } 
    }
    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }
    public DamageDetails TakeDamage(Move move, PokemonLevel attacker) /* formula to calculate the damage 
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
        float attack = (move.Base.IsSpecial) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.IsSpecial) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * type * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float) attack / defense);
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            damageDetails.Fainted = true;
        }

        return damageDetails;
    }
    public Move GetRandomMove()
    {
        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }
}

//show the player that they can know whether it was a super effective or not effective and a critical hit has landed in the battle dialog 
public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}
