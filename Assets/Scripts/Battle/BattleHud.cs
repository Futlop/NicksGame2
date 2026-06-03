using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;

    public void SetData(Creature creature)
    {
        nameText.text = creature.Base.Name;
        levelText.text = "Lv. " + creature.Level;
        hpBar.SetHP((float) creature.HP / creature.MaxHP);
    }
}
