using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionID = kvp.Key;
            var condition = kvp.Value;

            condition.ID = conditionID;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Creature creature) =>
                {
                    creature.UpdateHP(Mathf.Clamp(creature.MaxHP / 8, 1, creature.MaxHP));
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is hurt by poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                // TODO: Half creature's attack when burned
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Creature creature) =>
                {
                    creature.UpdateHP(Mathf.Clamp(creature.MaxHP / 16, 1, creature.MaxHP));
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is hurt by its burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Creature creature) =>
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        creature.StatusChanges.Enqueue($"{creature.Base.Name} is paralyzed. It can't move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                // TODO: half creature's special attack when frostbit
                Name = "Frostbite",
                StartMessage = "has frostbite",
                OnAfterTurn = (Creature creature) =>
                {
                    creature.UpdateHP(Mathf.Clamp(creature.MaxHP / 16, 1, creature.MaxHP));
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is hurt by its frostbite");
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Creature creature) =>
                {
                    // Sleep for 2-5 turns
                    creature.StatusTime = Random.Range(2, 6);
                },
                OnBeforeMove = (Creature creature) =>
                {
                    if(creature.StatusTime <= 0)
                    {
                        creature.CureStatus();
                        creature.StatusChanges.Enqueue($"{creature.Base.Name} woke up!");
                        return true;
                    }

                    creature.StatusTime--;
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is fast asleep");
                    return false;
                }
            }
        },
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has been confused",
                OnStart = (Creature creature) =>
                {
                    creature.VolatileStatusTime = Random.Range(1, 5);
                },
                OnBeforeMove = (Creature creature) =>
                {
                    if(creature.VolatileStatusTime <= 0)
                    {
                        creature.CureVolatileStatus();
                        creature.StatusChanges.Enqueue($"{creature.Base.Name} snapped out of its confusion!");
                        return true;
                    }

                    creature.VolatileStatusTime--;
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is confused");
                    
                    if(Random.Range(1, 3) == 1)
                        return true;
                    
                    creature.UpdateHP(Mathf.Clamp(creature.MaxHP / 8, 1, creature.MaxHP));
                    creature.StatusChanges.Enqueue($"It hurt itself due to confusion");
                    return false;
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, par, frz, slp, confusion
}