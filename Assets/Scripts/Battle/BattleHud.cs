using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] Text statusText;

    [SerializeField] Color psnColour;
    [SerializeField] Color brnColour;
    [SerializeField] Color parColour;
    [SerializeField] Color frzColour;
    [SerializeField] Color slpColour;
    
    Creature _creature;
    Dictionary<ConditionID, Color> statusColours;

    public void SetData(Creature creature)
    {
        _creature = creature;

        nameText.text = creature.Base.Name;
        levelText.text = "Lv. " + creature.Level;
        hpBar.SetHP((float) creature.HP / creature.MaxHP);

        statusColours = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColour},
            {ConditionID.brn, brnColour},
            {ConditionID.par, parColour},
            {ConditionID.frz, frzColour},
            {ConditionID.slp, slpColour}
        };

        SetStatusText();
        _creature.OnStatusChanged += SetStatusText;
    }

    void SetStatusText()
    {
        if(_creature.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _creature.Status.ID.ToString().ToUpper();
            statusText.color = statusColours[_creature.Status.ID];
        }
    }

    public IEnumerator UpdateHP()
    {
        if(_creature.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float) _creature.HP / _creature.MaxHP);
            _creature.HpChanged = false;
        }
    }
}
