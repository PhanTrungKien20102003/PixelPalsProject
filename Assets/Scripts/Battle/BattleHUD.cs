using System.Collections;
using System.Collections.Generic;
using TMPro;          //import TMPro to use the "Text - TextMeshPro" functions of Unity
using UnityEngine;
using UnityEngine.UI; //import UnityEngine.UI when working with UI

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    PokemonLevel _pokemon;
    public void SetData(PokemonLevel pokemon) //take PokemonLevel class
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv." + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp); //convert HP and MaxHP to float since both of them are integers and for the scale I use float
    }

    public IEnumerator UpdateHP() //update HP of the pokemon after taking damage
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }
}
