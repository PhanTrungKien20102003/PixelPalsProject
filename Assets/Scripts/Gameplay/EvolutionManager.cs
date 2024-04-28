using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image pokemonImage;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;
    
    public static EvolutionManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public IEnumerator Evolve(Pokemon pokemon, PokemonBase.Evolution evolution)
    {
        OnStartEvolution?.Invoke();
        evolutionUI.SetActive(true);
        
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{pokemon.Base.Name} is evolving!");

        var oldPokemon = pokemon.Base;
        pokemon.Evolve(evolution);
        
        pokemonImage.sprite = pokemon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldPokemon.Name} evolved into {pokemon.Base.Name}!");

        evolutionUI.SetActive(false);
        OnCompleteEvolution?.Invoke();
    }
}
