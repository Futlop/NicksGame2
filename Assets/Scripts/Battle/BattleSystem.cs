using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    int currentAction;
    int currentMove;
    int currentMember;

    Party playerParty;
    Creature wildCreature;

    public void StartBattle(Party playerParty, Creature wildCreature)
    {
        this.playerParty = playerParty;
        this.wildCreature = wildCreature;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyCreature());
        enemyUnit.Setup(wildCreature);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Creature.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Creature.Base.Name} appeared!");
        
        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if(playerUnit.Creature.Speed >= enemyUnit.Creature.Speed)
        {
            StartCoroutine(dialogBox.TypeDialog("Choose an action"));
            ActionSelection();
        }
        else
            StartCoroutine(EnemyMove());
    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Creatures.ForEach(c => c.OnBattleOver());
        OnBattleOver(won);
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.EnableDialogText(true);
        dialogBox.EnableActionSelector(true);
        dialogBox.EnableMoveSelector(false);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Creatures);
        partyScreen.gameObject.SetActive(true);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerUnit.Creature.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        if(state == BattleState.PerformMove) // If the battle state wasn't changed, go to the next step
            StartCoroutine(EnemyMove());
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Creature.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        if(state == BattleState.PerformMove) // If the battle state wasn't changed, go to the next step
        {
            yield return dialogBox.TypeDialog("Choose an action");
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Creature.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Creature);
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Creature);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Creature.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();

        if(move.Base.Category == MoveCategory.Status)
        {
           yield return RunMoveEffects(move, sourceUnit.Creature, targetUnit.Creature);
        }
        else
        {
            var damageDetails = targetUnit.Creature.TakeDamage(move, sourceUnit.Creature);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if (targetUnit.Creature.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Creature.Base.Name} fainted");
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(targetUnit);
        }

        // Statuses like poison can hurt the player's creature after attacking
        sourceUnit.Creature.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Creature);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Creature.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Creature.Base.Name} fainted");
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);

            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Creature source, Creature target)
    {
        var effects = move.Base.Effects;
        if(effects.Boosts != null) // Stat boosts
        {
            if(move.Base.Target == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if(effects.Status != ConditionID.none) // Status conditions
        {
            target.SetStatus(effects.Status);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Creature creature)
    {
        while(creature.StatusChanges.Count > 0)
        {
            var message = creature.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextCreature = playerParty.GetHealthyCreature();
            if(nextCreature != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            BattleOver(true);
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog("A critical hit!");

        if(damageDetails.TypeEffectiveness > 1f)
            yield return dialogBox.TypeDialog("It's super effective!");
        else if(damageDetails.TypeEffectiveness < 1f)
            yield return dialogBox.TypeDialog("It's not very effective");
    }

    public void HandleUpdate()
    {
        if(state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if(state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if(currentAction == 0) //Fight is selected
            {
                MoveSelection();
            }
            else if(currentAction == 1) //Bag is selected
            {
                
            }
            else if(currentAction == 2) //Creatures is selected
            {
                OpenPartyScreen();
            }
            else if(currentAction == 3) //Run is selected
            {
                
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Creature.Moves.Count - 1);

        dialogBox.UpdayeMoveSelection(currentMove, playerUnit.Creature.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Creatures.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var selectedMember = playerParty.Creatures[currentMember];
            if(selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText($"{selectedMember.Base.Name} is unable to battle");
                return;
            }
            if(selectedMember == playerUnit.Creature)
            {
                partyScreen.SetMessageText($"{selectedMember.Base.Name} is already in battle");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            dialogBox.EnableActionSelector(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchCreature(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchCreature(Creature newCreature)
    {
        bool currentCreatureFainted = true;
        if(playerUnit.Creature.HP > 0)
        {
            currentCreatureFainted = false;
            yield return dialogBox.TypeDialog($"Come back, {playerUnit.Creature.Base.Name}!");
            playerUnit.Creature.OnBattleOver();
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newCreature);
        dialogBox.SetMoveNames(newCreature.Moves);
        yield return dialogBox.TypeDialog($"Go {newCreature.Base.Name}!");

        if(currentCreatureFainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());
    }
}
