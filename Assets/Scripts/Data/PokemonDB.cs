using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonDB
{ 
    static Dictionary<string, PokemonBase> pokemons;

    public static void Init()
    {
        pokemons = new Dictionary<string, PokemonBase>();
        
        //this function will only load the objects that are inside a folder call Resources
        var pokemonArray = Resources.LoadAll<PokemonBase>("");
        foreach (var pokemon in pokemonArray)
        {
            if (pokemons.ContainsKey(pokemon.Name))
            {
                Debug.LogError($"There is already a Pokemon called {pokemon.Name}!");
                continue;
            }
            
            pokemons[pokemon.Name] = pokemon;
        }
    }
    public static PokemonBase GetPokemonByName(string name)
    {
        if (!pokemons.ContainsKey(name))
        {
            Debug.LogError($"Pokemon with the name {name} not found in database!");
            return null;
        }
        return pokemons[name];
    }
}
