using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Creature", menuName = "Creature/Create new creature")]
public class CreatureBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] Sprite front;
    [SerializeField] Sprite back;

    [SerializeField] Type type1;
    [SerializeField] Type type2;

    [SerializeField] int maxHP;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;

    [SerializeField] List<LearnableMove> learnableMoves;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite Front
    {
        get { return front; }
    }

    public Sprite Back
    {
        get { return back; }
    }

    public Type Type1
    {
        get { return type1; }
    }

    public Type Type2
    {
        get { return type2; }
    }

    public int MaxHP
    {
        get { return maxHP; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum Type
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon,
    Dark,
    Steel,
    Fairy
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed
}

public class TypeChart
{
    static float[][] chart =
    {   //                    NOR  FIR  WAT  ELE  GRS  ICE  FGT  PSN  GRD  FLY  PSY  BUG  RCK  GST  DGN  DRK  STL  FRY
        /*NOR*/ new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  0.5f,0f,  1f,  1f,  0.5f,1f },
        /*FIR*/ new float[] { 1f,  0.5f,0.5f,1f,  2f,  2f,  1f,  1f,  1f,  1f,  1f,  2f,  0.5f,1f,  0.5f,1f,  2f,  1f  },
        /*WAT*/ new float[] { 1f,  2f,  0.5f,1f,  0.5f,1f,  1f,  1f,  2f,  1f,  1f,  1f,  2f,  1f,  0.5f,1f,  1f,  1f  },
        /*ELE*/ new float[] { 1f,  1f,  2f,  0.5f,0.5f,1f,  1f,  1f,  0f,  2f,  1f,  1f,  1f,  1f,  0.5f,1f,  1f,  1f  },
        /*GRS*/ new float[] { 1f,  0.5f,2f,  1f,  0.5f,1f,  1f,  0.5f,2f,  0.5f,1f,  0.5f,2f,  1f,  0.5f,1f,  0.5f,1f  },
        /*ICE*/ new float[] { 1f,  0.5f,0.5f,1f,  2f,  0.5f,1f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  2f,  1f,  0.5f,1f  },
        /*FGT*/ new float[] { 2f,  1f,  1f,  1f,  1f,  2f,  1f,  0.5f,1f,  0.5f,0.5f,0.5f,2f,  0f,  1f,  2f,  2f,  0.5f},
        /*PSN*/ new float[] { 1f,  1f,  1f,  1f,  2f,  1f,  1f,  0.5f,0.5f,1f,  1f,  1f,  0.5f,0.5f,1f,  1f,  0f,  2f  },
        /*GRD*/ new float[] { 1f,  2f,  1f,  2f,  0.5f,1f,  1f,  2f,  1f,  0f,  1f,  0.5f,2f,  1f,  1f,  1f,  2f,  1f  },
        /*FLY*/ new float[] { 1f,  1f,  1f,  0.5f,2f,  1f,  2f,  1f,  1f,  1f,  1f,  2f,  0.5f,1f,  1f,  1f,  0.5f,1f  },
        /*PSY*/ new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f,  0.5f,1f,  1f,  1f,  1f,  0f,  0.5f,1f  },
        /*BUG*/ new float[] { 1f,  0.5f,1f,  1f,  2f,  1f,  0.5f,0.5f,1f,  0.5f,2f,  1f,  1f,  0.5f,1f,  2f,  0.5f,0.5f},
        /*RCK*/ new float[] { 1f,  2f,  1f,  1f,  1f,  2f,  0.5f,1f,  0.5f,2f,  1f,  2f,  1f,  1f,  1f,  1f,  0.5f,1f  },
        /*GST*/ new float[] { 0f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f,  0.5f,1f,  1f  },
        /*DGN*/ new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  0.5f,0f  },
        /*DRK*/ new float[] { 1f,  1f,  1f,  1f,  1f,  1f,  0.5f,1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f,  0.5f,0.5f,0.5f},
        /*STL*/ new float[] { 1f,  0.5f,0.5f,0.5f,1f,  2f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f,  0.5f,2f  },
        /*FRY*/ new float[] { 1f,  0.5f,1f,  1f,  1f,  1f,  2f,  0.5f,1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f,  0.5f,1f  },
    };

    public static float GetEffectiveness(Type attackType, Type defenseType)
    {
        if(attackType == Type.None || defenseType == Type.None)
            return 1;

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];   
    }
}
