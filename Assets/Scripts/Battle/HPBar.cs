using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health; //a reference to the image that is showing the health
    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f); 
    }
    public IEnumerator SetHPSmooth(float newHP) //decrease the HP slowly and smoothly
    {
        float curHP = health.transform.localScale.x; //current HP
        float changeAmt = curHP - newHP; //calculate the amount of HP that we have to change
        
        while (curHP - newHP > Mathf.Epsilon) //a loop that will run until the difference between the current HP and the new HP is a small value
        {
            curHP -= changeAmt * Time.deltaTime; /* reduce the current HP by a small amount
                                                    multiplying the change amount with Time.deltaTime
                                                    will only take a small portion of the change amount */

            health.transform.localScale = new Vector3(curHP, 1f); //set the current HP as the scale of the health bar in the UI
            yield return null;
        }
        health.transform.localScale = new Vector3(newHP, 1f); //new HP
    }
}
