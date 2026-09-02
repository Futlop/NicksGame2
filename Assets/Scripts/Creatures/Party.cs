using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Party : MonoBehaviour
{
    [SerializeField] List<Creature> creatures;

    public List<Creature> Creatures
    {
        get
        {
            return creatures;
        }
    }

    private void Start()
    {
        foreach(var creature in creatures)
        {
            creature.Init();
        }
    }

    // Returns first creature whose HP is greater than 0.  Returns null if none are found
    public Creature GetHealthyCreature()
    {
        return creatures.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddCreature(Creature newCreature)
    {
        if(creatures.Count < 6)
        {
            creatures.Add(newCreature);
        }
        else
        {
            // TODO: Add storage for creatures not in party
        }
    }
}
