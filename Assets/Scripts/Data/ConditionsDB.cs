using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Creature creature) =>
                {
                    creature.UpdateHP(creature.MaxHP / 8);
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is hurt by poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition
            {
                Name = "Burn",
                StartMessage = "has been burn",
                OnAfterTurn = (Creature creature) =>
                {
                    creature.UpdateHP(creature.MaxHP / 16);
                    creature.StatusChanges.Enqueue($"{creature.Base.Name} is hurt by its burn");
                }
            }
        }
    };
}

public enum ConditionID
{
    none, psn, brn, par, frz
}