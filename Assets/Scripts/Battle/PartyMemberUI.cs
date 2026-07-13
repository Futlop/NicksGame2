using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Color highlightedColour;
    
    Creature _creature;

    public void SetData(Creature creature)
    {
        _creature = creature;

        nameText.text = creature.Base.Name;
        levelText.text = "Lv. " + creature.Level;
        hpBar.SetHP((float) creature.HP / creature.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if(selected)
            nameText.color = highlightedColour;
        else
            nameText.color = Color.black;
    }
}
