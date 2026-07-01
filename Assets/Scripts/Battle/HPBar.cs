using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;
    
    public void SetHP(float HPNormalized)
    {
        health.transform.localScale = new Vector3(HPNormalized, 1f);

        if(HPNormalized >= 0.5)
        {
            health.GetComponent<Image>().color = new Color(((HPNormalized-1)*-2), 1, 0);
        }
        else
        {
            health.GetComponent<Image>().color = new Color(1, ((HPNormalized)*2), 0);
        }
    }

    // Change health bar colour based on remaining HP.  Green at 100%, becomes gradually more yellow at 50%, becomes gradually more red at 0%
    void updateHPColour(float curHP)
    {
        if(curHP >= 0.5)
        {
            health.GetComponent<Image>().color = new Color(((1-curHP)*2), 1, 0);
        }
        else
        {
            health.GetComponent<Image>().color = new Color(1, ((curHP)*2), 0);
        }
    }

    public IEnumerator SetHPSmooth(float newHP)
    {
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHP;

        while(curHP - newHP > Mathf.Epsilon)
        {
            curHP -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHP, 1f);
            updateHPColour(curHP);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHP, 1f);
        updateHPColour(newHP);
    }
}
