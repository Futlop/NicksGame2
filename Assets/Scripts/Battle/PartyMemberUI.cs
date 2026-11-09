using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    
    Creature _creature;

    public void Init(Creature creature)
    {
        _creature = creature;
        UpdateData();

        _creature.OnHPChanged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = _creature.Base.Name;
        levelText.text = "Lv. " + _creature.Level;
        hpBar.SetHP((float) _creature.HP / _creature.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if(selected)
            nameText.color = GlobalSettings.i.HighlightedColour;
        else
            nameText.color = Color.black;
    }
}
