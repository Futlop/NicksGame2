using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] GameObject expBar;
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
        SetLevel();
        hpBar.SetHP((float) creature.HP / creature.MaxHP);
        SetExp();

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

    public void SetLevel()
    {
        levelText.text = "Lv. " + _creature.Level;
    }

    public void SetExp()
    {
        if(expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if(expBar == null) yield break;

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    float GetNormalizedExp()
    {
        int currentLvlExp = _creature.Base.GetExpForLevel(_creature.Level);
        int nextLvlExp = _creature.Base.GetExpForLevel(_creature.Level + 1);

        float normalizedExp = (float)(_creature.Exp - currentLvlExp) / (nextLvlExp - currentLvlExp);
        return Mathf.Clamp01(normalizedExp);
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
