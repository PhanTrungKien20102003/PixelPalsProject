using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB //store instances of Conditions class
{   
    public static void Init()
    {
        foreach (var key_value_pair in Conditions) //loop through elements in COnditions Dictionary
        {
            var condition_id = key_value_pair.Key;
            var condition = key_value_pair.Value;

            condition.id = condition_id;
        }    
    }
    public static Dictionary<ConditionID, Conditions> Conditions { get; set; } = new Dictionary<ConditionID, Conditions>()
                                                                                //initialize the dictionary
    {
        {
            //define status conditions
            ConditionID.PSN,
            new Conditions()
            {
                Name = "Poison",
                StartMessage = "is now poisoned!",

                //using lambda function to define the function here itself while assigning it
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by poison!");
                }
            }
        },
        {
            ConditionID.BRN,
            new Conditions()
            {
                Name = "Burn",
                StartMessage = "is now burned!",
                OnAfterTurn = (Pokemon pokemon) =>
                {
                    pokemon.DecreaseHP(pokemon.MaxHp / 16);
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is hurt by its burn!");
                }
            }
        },
        {
            ConditionID.PAR,
            new Conditions()
            {
                Name = "Paralyzed",
                StartMessage = "is paralyzed! " +
                "\nIt may be unable to move!",
                OnBeforeMove = (Pokemon pokemon) => 
                {
                    if (Random.Range(1,5) == 1) //if the pokemon is paralyze, there is 25% for them to not perform a move
                    {
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is paralyzed! It can't move!");
                        return false;
                    }

                    return true;
                }
            }
        },
        {
            ConditionID.FRZ,
            new Conditions()
            {
                Name = "Freeze",
                StartMessage = "became frozen!",
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (Random.Range(1,11) == 1) //if the pokemon is frozen, there is 10% for them to thaw out
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is no longer frozen");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.SLP ,
            new Conditions()
            {
                Name = "Sleep",
                StartMessage = "is now fallen asleep!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Sleep for 1-3 turn(s)
                    pokemon.StatusTime = Random.Range(1,4);
                    Debug.Log($"Will be asleep for {pokemon.StatusTime} moves!");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.StatusTime <= 0)
                    {
                        pokemon.CureStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} woke up!");
                        return true;
                    }

                    pokemon.StatusTime--;
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is fast asleep!");
                    return false;
                }
            }
        },


        //volatile status conditions
        {
            ConditionID.Confused ,
            new Conditions()
            {
                Name = "Confusion",
                StartMessage = "is confused!",
                OnStart = (Pokemon pokemon) =>
                {
                    //Confused for 1-4 turn(s)
                    pokemon.VolatileStatusTime = Random.Range(1,5);
                    Debug.Log($"Will be confused for {pokemon.VolatileStatusTime} moves!");
                },
                OnBeforeMove = (Pokemon pokemon) =>
                {
                    if (pokemon.VolatileStatusTime <= 0)
                    {
                        pokemon.CureVolatileStatus();
                        pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} kicked out of confusion!");
                        return true;
                    }
                    pokemon.VolatileStatusTime--;

                    //50% chance to perform a move
                    if (Random.Range(1,3) == 1)
                    {
                        return true;
                    }

                    //Hurt by confusion
                    pokemon.StatusChanges.Enqueue($"{pokemon.Base.Name} is confused!");
                    pokemon.DecreaseHP(pokemon.MaxHp / 8);
                    pokemon.StatusChanges.Enqueue($"It hurt itself due to its confusion!");
                    return false;
                }
            }
        },
    };

    public static float GetStatusBonus(Conditions conditions)
    {
        if (conditions == null)
            return 1f;
        else if (conditions.id == ConditionID.SLP || conditions.id == ConditionID.FRZ)
            return 2f;
        else if (conditions.id == ConditionID.PSN || conditions.id == ConditionID.PAR || conditions.id == ConditionID.BRN)
            return 1.5f;

        return 1f;
    }
}
public enum ConditionID
{
    //PSN = Poison; BRN = Burned; SLP = Sleep; PAR = Paralyze; FRZ = Frozen
    None, PSN, BRN, SLP, PAR, FRZ,
    Confused
}