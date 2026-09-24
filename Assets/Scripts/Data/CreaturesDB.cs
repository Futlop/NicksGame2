using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesDB
{
    static Dictionary<string, CreatureBase> creatures;

    public static void Init()
    {
        creatures = new Dictionary<string, CreatureBase>();

        var creatureArray = Resources.LoadAll<CreatureBase>("");
        foreach(var creature in creatureArray)
        {
            if (creatures.ContainsKey(creature.Name))
            {
                Debug.LogError($"There are multiple creatures with the name {creature.Name}");
                continue;
            }

            creatures[creature.Name] = creature;
        }
    }

    public static CreatureBase GetCreatureByName(string name)
    {
        if (!creatures.ContainsKey(name))
        {
            Debug.LogError($"No creature found with name: {name}");
            return null;
        }

        return creatures[name];
    }
}
