using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highlightedColor;

    Pokemon _pokemon;
    public void SetData(Pokemon pokemon) //take PokemonLevel class
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv." + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp); 
    }

    public void SetSelected(bool selected) //highlight member when choosing
    {
        if (selected)
        {
            nameText.color = highlightedColor;
        } else
        {
            nameText.color = Color.black;
        }
    }
}
