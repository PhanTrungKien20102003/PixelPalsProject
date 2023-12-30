using System.Collections;
using System.Collections.Generic;
using TMPro;          //import TMPro to use the "Text - TextMeshPro" functions of Unity
using UnityEngine;
using UnityEngine.UI; //import UnityEngine.UI when working with UI

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;

    //set different color for different statuses
    [SerializeField] Color PSN_Color;
    [SerializeField] Color PAR_Color;
    [SerializeField] Color BRN_Color;
    [SerializeField] Color SLP_Color;
    [SerializeField] Color FRZ_Color;

    Pokemon _pokemon;

    Dictionary<ConditionID, Color> statusColors;
    public void SetData(Pokemon pokemon) //take PokemonLevel class
    {
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        levelText.text = "Lv." + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp); //convert HP and MaxHP to float since both of them are integers and for the scale I use float

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.PSN, PSN_Color },
            {ConditionID.PAR, PAR_Color },
            {ConditionID.BRN, BRN_Color },
            {ConditionID.SLP, SLP_Color },
            {ConditionID.FRZ, FRZ_Color },
        };

        SetStatusText();
        _pokemon.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if (_pokemon.Status == null) 
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _pokemon.Status.id.ToString().ToUpper();
            statusText.color = statusColors[_pokemon.Status.id];
        }
    }

    public IEnumerator UpdateHP() //update HP of the pokemon after taking damage
    {
          if (_pokemon.HPChanged)
        {
            yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
            _pokemon.HPChanged = false;
        }
    }
}
