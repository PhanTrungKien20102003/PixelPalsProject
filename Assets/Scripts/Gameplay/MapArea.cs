using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapArea : MonoBehaviour
{
    [SerializeField] List<Pokemon> wildPokemons;

    public Pokemon GetRandomWildPokemon()
    {
        //just have to initialize them in this function because they're only relevant in the battle
        var wildPokemon = wildPokemons[Random.Range(0, wildPokemons.Count)];
                                                       //size of the list
        wildPokemon.Init();
        return wildPokemon;
    }
}
