using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq; //import Linq
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List <Pokemon> pokemons; //make it SerializedField so that I can set it from the Inspector

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get { return pokemons; }
        set { pokemons = value; }
    }

    private void Start()
    {
        foreach (var pokemon in pokemons) //loop through all the pokemons and initialize each one of them
        {
            pokemon.Init();
        }
    }
    public Pokemon GetHealthyPokemon() //pass the first Pokemon in the party that isn't fainted
    {
        /*the Where() function will loop through the list of Pokemons we have and it will return the list of Pokemon
          which satisfies the conditions => return all the Pokemons that are not fainted in party*/
        return pokemons.Where(x => x.HP > 0).FirstOrDefault(); 
                                            //just need the first one that isn't fainted so I used FirstOrDefault()
    }

    public void AddPokemon(Pokemon newPokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newPokemon);
            OnUpdated?.Invoke();            
        }
        else
        {
            // TODO: Add PC once that implemented
        }
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
