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


    Pokemon _pokemon;
    public void Init(Pokemon pokemon) //take PokemonLevel class
    {
        _pokemon = pokemon;
        UpdateData();
        
        _pokemon.OnHPChanged += UpdateData;
        
    }

    void UpdateData()
    {
        nameText.text = _pokemon.Base.Name;
        levelText.text = "Lv." + _pokemon.Level;
        hpBar.SetHP((float)_pokemon.HP / _pokemon.MaxHp); 
    }

    public void SetSelected(bool selected) //highlight member when choosing
    {
        if (selected)
            nameText.color = GlobalSettings.instance.HighlightedColor;
        else
            nameText.color = Color.black;
    }
}
