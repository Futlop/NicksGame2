using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, AboutToUse, BattleOver}
public enum BattleAction { Move, SwitchCreature, UseItem, Run}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject ballSprite;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
    int currentAction;
    int currentMove;
    int currentMember;
    bool aboutToUseChoice = true;
    int escapeAttempts;

    Party playerParty;
    Party trainerParty;
    Creature wildCreature;

    bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    public void StartBattle(Party playerParty, Creature wildCreature)
    {
        isTrainerBattle = false;
        this.playerParty = playerParty;
        this.wildCreature = wildCreature;
        player = playerParty.GetComponent<PlayerController>();
        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(Party playerParty, Party trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;

        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();

        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Clear();
        enemyUnit.Clear();

        if (!isTrainerBattle)
        {
            // Wild creature battle
            playerUnit.Setup(playerParty.GetHealthyCreature());
            enemyUnit.Setup(wildCreature);

            dialogBox.SetMoveNames(playerUnit.Creature.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Creature.Base.Name} appeared!");
        }
        else
        {
            // Trainer battle
            // Show player and trainer sprites
            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);
            
            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle!");

            // Trainer sends out first creature
            trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyCreature = trainerParty.GetHealthyCreature();
            enemyUnit.Setup(enemyCreature);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyCreature.Base.Name}");

            // Player sends out first creature
            playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerCreature = playerParty.GetHealthyCreature();
            playerUnit.Setup(playerCreature);
            yield return dialogBox.TypeDialog($"Go, {playerCreature.Base.Name}!");
            dialogBox.SetMoveNames(playerUnit.Creature.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        yield return dialogBox.TypeDialog($"Choose an action");
        ActionSelection();
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

    IEnumerator AboutToUse(Creature newCreature)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newCreature.Base.Name}.  Would you like to switch creatures?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;

        if(playerAction == BattleAction.Move)
        {
            playerUnit.Creature.CurrentMove = playerUnit.Creature.Moves[currentMove];
            enemyUnit.Creature.CurrentMove = enemyUnit.Creature.GetRandomMove();

            int playerMovePriority = playerUnit.Creature.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Creature.CurrentMove.Base.Priority;

            // Check who goes first
            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
                playerGoesFirst = false;
            else if(enemyMovePriority == playerMovePriority)
                playerGoesFirst = playerUnit.Creature.Speed >= enemyUnit.Creature.Speed;

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondCreature = secondUnit.Creature;

            // First turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Creature.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if(state == BattleState.BattleOver) yield break;

            // Second turn
            if(secondCreature.HP > 0)
            {
                yield return RunMove(secondUnit, firstUnit, secondUnit.Creature.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if(state == BattleState.BattleOver) yield break;
            }            
        }
        else
        {
            if(playerAction == BattleAction.SwitchCreature)
            {
                var selectedCreature = playerParty.Creatures[currentMember];
                state = BattleState.Busy;
                yield return SwitchCreature(selectedCreature);
            }
            else if(playerAction == BattleAction.UseItem)
            {
                dialogBox.EnableActionSelector(false);
                yield return ThrowBall();
            }
            else if(playerAction == BattleAction.Run)
            {
                dialogBox.EnableActionSelector(false);
                yield return TryToEscape();
            }

            // Enemy turn
            var enemyMove = enemyUnit.Creature.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if(state == BattleState.BattleOver) yield break;
        }

        if(state != BattleState.BattleOver)
            ActionSelection();
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Creature.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Creature);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Creature);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Creature.Base.Name} used {move.Base.Name}");

        if(CheckIfMoveHits(move, sourceUnit.Creature, targetUnit.Creature))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if(move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Creature, targetUnit.Creature, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Creature.TakeDamage(move, sourceUnit.Creature);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.Secondaries != null && move.Base.Secondaries.Count > 0 && targetUnit.Creature.HP > 0)
            {
                foreach(var secondary in move.Base.Secondaries)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if(rnd <= secondary.Chance)
                        yield return RunMoveEffects(secondary, sourceUnit.Creature, targetUnit.Creature, secondary.Target);
                }
            }

            if (targetUnit.Creature.HP <= 0)
            {
                yield return HandleCreatureFainted(targetUnit);
            }
        }
        else
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Creature.Base.Name} evaded the attack");
        }
    }

    IEnumerator RunMoveEffects(MoveEffects effects, Creature source, Creature target, MoveTarget moveTarget)
    {
        if(effects.Boosts != null) // Stat boosts
        {
            if(moveTarget == MoveTarget.Self)
                source.ApplyBoosts(effects.Boosts);
            else
                target.ApplyBoosts(effects.Boosts);
        }

        if(effects.Status != ConditionID.none) // Status conditions
        {
            target.SetStatus(effects.Status);
        }

        if(effects.VolatileStatus != ConditionID.none) // Volatile status conditions
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if(state == BattleState.BattleOver) yield break;
        yield return new WaitUntil(() => state == BattleState.RunningTurn); // Pauses execution until state is equal to RunningTurn
        
        // Statuses like poison can hurt the player's creature after attacking
        sourceUnit.Creature.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Creature);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Creature.HP <= 0)
        {
            yield return HandleCreatureFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
        }
    }

    bool CheckIfMoveHits(Move move, Creature source, Creature target)
    {
        if(move.Base.AlwaysHits)
            return true;

        float moveAccuracy = move.Base.Accuracy;
        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if(accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];

        if(evasion > 0)
            moveAccuracy /= boostValues[evasion];
        else
            moveAccuracy *= boostValues[-evasion];

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
    }

    IEnumerator ShowStatusChanges(Creature creature)
    {
        while(creature.StatusChanges.Count > 0)
        {
            var message = creature.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    IEnumerator HandleCreatureFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Creature.Base.Name} fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);

        if (!faintedUnit.IsPlayerUnit)
        {
            // Exp gain
            int expYield = faintedUnit.Creature.Base.ExpYield;
            int enemyLevel = faintedUnit.Creature.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Creature.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Creature.Base.Name} gained {expGain} exp. points!");
            yield return playerUnit.Hud.SetExpSmooth();

            // Check lvl up
            while (playerUnit.Creature.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Creature.Base.Name} grew to level {playerUnit.Creature.Level}!");

                // Try to learn a new move
                var newMove = playerUnit.Creature.GetLearnableMoveAtCurrentLevel();
                if(newMove != null)
                {
                    if(playerUnit.Creature.Moves.Count < CreatureBase.maxMoves)
                    {
                        playerUnit.Creature.LearnMove(newMove);
                        yield return dialogBox.TypeDialog($"{playerUnit.Creature.Base.Name} learned {newMove.Base.Name}!");
                        dialogBox.SetMoveNames(playerUnit.Creature.Moves);
                    }
                    else
                    {
                        
                    }
                }

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
            if(!isTrainerBattle)
                BattleOver(true);
            else
            {
                var nextCreature = trainerParty.GetHealthyCreature();
                if(nextCreature != null)
                    StartCoroutine(AboutToUse(nextCreature));
                else
                    BattleOver(true);
            }
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
        if(state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
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
                StartCoroutine(RunTurns(BattleAction.UseItem));
            }
            else if(currentAction == 2) //Creatures is selected
            {
                prevState = state;
                OpenPartyScreen();
            }
            else if(currentAction == 3) //Run is selected
            {
                StartCoroutine(RunTurns(BattleAction.Run));
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

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Creature.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            var move = playerUnit.Creature.Moves[currentMove];
            if(move.PP == 0) return;

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
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

            if(prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchCreature));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchCreature(selectedMember));
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            if(playerUnit.Creature.HP <= 0)
            {
                partyScreen.SetMessageText("You need to choose a creature to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);

            if(prevState == BattleState.AboutToUse)
            {
                prevState = null;
                StartCoroutine(SendNextTrainerCreature());
            }
            else
                ActionSelection();
        }
    }

    void HandleAboutToUse()
    {
        if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            aboutToUseChoice = !aboutToUseChoice;

        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            dialogBox.EnableChoiceBox(false);
            if (aboutToUseChoice)
            {
                // Yes option
                prevState = BattleState.AboutToUse;
                OpenPartyScreen();
            }
            else
            {
                // No option
                StartCoroutine(SendNextTrainerCreature());
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerCreature());
        }
    }

    IEnumerator SwitchCreature(Creature newCreature)
    {
        if(playerUnit.Creature.HP > 0)
        {
            playerUnit.Creature.CureVolatileStatus();
            yield return dialogBox.TypeDialog($"Come back, {playerUnit.Creature.Base.Name}!");
            playerUnit.Creature.OnBattleOver();
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newCreature);
        dialogBox.SetMoveNames(newCreature.Moves);
        yield return dialogBox.TypeDialog($"Go {newCreature.Base.Name}!");

        if(prevState == null)
        {
            state = BattleState.RunningTurn;
        }
        else if(prevState == BattleState.AboutToUse)
        {
            prevState = null;
            StartCoroutine(SendNextTrainerCreature());
        }
    }

    IEnumerator SendNextTrainerCreature()
    {
        state = BattleState.Busy;

        var nextCreature = trainerParty.GetHealthyCreature();
        enemyUnit.Setup(nextCreature);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextCreature.Base.Name}");

        yield return dialogBox.TypeDialog("Choose an action");
        state = BattleState.RunningTurn;
    }

    IEnumerator ThrowBall()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("You can't steal a trainer's creature!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} used a Basic Ball!");

        var ballObj = Instantiate(ballSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var ball = ballObj.GetComponent<SpriteRenderer>();

        // Animations
        yield return ball.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return ball.transform.DOMoveY(enemyUnit.transform.position.y - 1.5f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchCreature(enemyUnit.Creature);

        for(int i = 0; i < Mathf.Min(shakeCount, 3); i++)
        {
            yield return new WaitForSeconds(0.5f);
            yield return ball.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if(shakeCount == 4)
        {
            // Successful Capture
            yield return dialogBox.TypeDialog($"{enemyUnit.Creature.Base.Name} was caught!");
            yield return ball.DOFade(0, 1.5f).WaitForCompletion();

            playerParty.AddCreature(enemyUnit.Creature);
            yield return dialogBox.TypeDialog($"{enemyUnit.Creature.Base.Name} was added to your party!");

            Destroy(ball);
            Destroy(ballObj);
            BattleOver(true);
        }
        else
        {
            // Creature broke free
            yield return new WaitForSeconds(1f);
            ball.DOFade(0, 0.2f);
            yield return enemyUnit.PlayBreakoutAnimation();

            if(shakeCount < 2)
                yield return dialogBox.TypeDialog($"{enemyUnit.Creature.Base.Name} broke free");
            else
                yield return dialogBox.TypeDialog("Almost had it");

            Destroy(ball);
            Destroy(ballObj);
            state = BattleState.RunningTurn;
        }
    }

    int TryToCatchCreature(Creature creature)
    {
        float a = (3 * creature.MaxHP - 2 * creature.HP) * creature.Base.CatchRate * ConditionsDB.GetStatusBonus(creature.Status) / (3 * creature.MaxHP);

        if(a >= 255)
            return 4;
        
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while(shakeCount < 4)
        {
            if(UnityEngine.Random.Range(0, 65535) >= b)
                break;

            shakeCount++;
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;

        if (isTrainerBattle)
        {
            yield return dialogBox.TypeDialog("You can't run from trainer battles");
            state = BattleState.RunningTurn;
            yield break;
        }

        escapeAttempts++;

        int playerSpeed = playerUnit.Creature.Speed;
        int enemySpeed = enemyUnit.Creature.Speed;

        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog("Got away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog("Got away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog("Couldn't get away");
                state = BattleState.RunningTurn;
            }
        }
    }
}
