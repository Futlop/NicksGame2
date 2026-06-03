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

        // Change health bar colour based on remaining HP.  Green at 100%, becomes gradually more yellow at 50%, becomes gradually more red at 0%
        if(HPNormalized >= 0.5)
        {
            health.GetComponent<Image>().color = new Color(((HPNormalized-1)*-2), 1, 0);
        }
        else
        {
            health.GetComponent<Image>().color = new Color(1, ((HPNormalized)*2), 0);
        }
    }
}
