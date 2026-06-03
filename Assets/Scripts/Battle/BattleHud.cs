using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    private float percent = 1;

    public void SetData(Creature creature)
    {
        nameText.text = creature.Base.Name;
        levelText.text = "Lv. " + creature.Level;
        hpBar.SetHP((float) creature.HP / creature.MaxHP);
    }

    public void Update()
    {
        if(percent >= 0f)
        {
            hpBar.SetHP(percent);
            percent -= 0.001f;
        }
    }
}
