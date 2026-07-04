using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Creature/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] Type type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Type Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }
    public int Accuracy
    {
        get { return accuracy; }
    }
    public int PP
    {
        get { return pp; }
    }

    public bool IsSpecial
    {
        get
        {
            if(type == Type.Fire || type == Type.Water || type == Type.Grass || type == Type.Ice || type == Type.Electric || type == Type.Dragon || type == Type.Fairy)
                return true;
            else
                return false;
        }
    }
}
