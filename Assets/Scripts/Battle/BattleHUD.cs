using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;          //import TMPro to use the "Text - TextMeshPro" functions of Unity
using UnityEngine;
using UnityEngine.UI; //import UnityEngine.UI when working with UI

public class BattleHUD : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;

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
        if (_pokemon != null)
        {
            _pokemon.OnHPChanged -= UpdateHP;
            _pokemon.OnStatusChanged -= SetStatusText;

        }
        
        _pokemon = pokemon;

        nameText.text = pokemon.Base.Name;
        SetLevel();
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHp); //convert HP and MaxHP to float since both of them are integers and for the scale I use float
        SetExp();
        
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
        _pokemon.OnHPChanged += UpdateHP;
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

    public void SetLevel()
    {
        levelText.text = "Lv." + _pokemon.Level;
    }

    public void SetExp()
    {
        if (expBar == null)
            return;
        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }
    
    public IEnumerator SetExpSmoothly(bool reset = false)
    {
        if (expBar == null)
            yield break;
        
        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currentLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level);
        int nextLevelExp = _pokemon.Base.GetExpForLevel(_pokemon.Level + 1);
        
        float normalizedExp = (float)(_pokemon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }
    public IEnumerator UpdateHPAsync() //update HP of the pokemon after taking damage
    {
        yield return hpBar.SetHPSmooth((float)_pokemon.HP / _pokemon.MaxHp);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }
}
