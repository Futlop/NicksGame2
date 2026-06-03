using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] CreatureBase _base;
    [SerializeField] int level;
    [SerializeField] bool isPlayerUnit;

    public Creature Creature { get; set; }

    public void Setup()
    {
        Creature = new Creature(_base, level);
        if (isPlayerUnit)
            GetComponent<Image>().sprite = Creature.Base.Back;
        else
            GetComponent<Image>().sprite = Creature.Base.Front;
    }
}
