using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    private enum BattleActionMode
    {
        None,
        SelectingBasicAttackTarget,
        SelectingSkillTarget
    }

    [SerializeField] private List<CharacterData> playerCharacters = new();
    [SerializeField] private List<CharacterData> enemyCharacters = new();
    [SerializeField] private List<BattleUnitView> playerUnitViews = new();
    [SerializeField] private List<BattleUnitView> enemyUnitViews = new();
    [SerializeField] private List<TargetButton> enemyTargetButtons = new();
    [SerializeField] private List<WorldTarget3D> enemyWorldTargets = new();
    [SerializeField] private GameObject battleResultPanel;
    [SerializeField] private TextMeshProUGUI battleResultText;
    [SerializeField] private TextMeshProUGUI turnIndicatorText;

    private readonly List<BattleUnit> battleUnits = new();
    private readonly List<BattleUnit> playerBattleUnits = new();
    private readonly List<BattleUnit> enemyBattleUnits = new();
    private readonly Dictionary<TargetButton, int> enemyTargetButtonIndices = new();
    private int currentTurnIndex;
    private BattleActionMode currentActionMode = BattleActionMode.None;
    private bool isBattleOver;
    private Coroutine enemyTurnCoroutine;

    private void Start()
    {
        playerCharacters ??= new List<CharacterData>();
        enemyCharacters ??= new List<CharacterData>();

        if (playerCharacters.Count != 3)
        {
            Debug.LogWarning($"BattleManager expected 3 player characters but found {playerCharacters.Count}.");
        }

        if (enemyCharacters.Count != 3)
        {
            Debug.LogWarning($"BattleManager expected 3 enemy characters but found {enemyCharacters.Count}.");
        }

        battleUnits.Clear();
        playerBattleUnits.Clear();
        enemyBattleUnits.Clear();
        isBattleOver = false;

        if (battleResultPanel != null)
        {
            battleResultPanel.SetActive(false);
        }

        UpdateTurnIndicator(string.Empty);

        AddBattleUnits(playerCharacters, playerBattleUnits);
        AddBattleUnits(enemyCharacters, enemyBattleUnits);

        battleUnits.AddRange(playerBattleUnits);
        battleUnits.AddRange(enemyBattleUnits);

        var orderedUnits = battleUnits
            .OrderByDescending(unit => unit.Speed)
            .ToList();

        battleUnits.Clear();
        battleUnits.AddRange(orderedUnits);

        currentTurnIndex = 0;

        if (battleUnits.Count == 0)
        {
            Debug.Log("Turn order: no valid battle units found.");
            return;
        }

        var turnOrder = string.Join(", ", battleUnits.Select((unit, index) =>
            $"{index + 1}. {unit.Name} ({unit.Team}) SPD {unit.Speed}"));

        BindUnitViews();
        SetupTargetButtons();
        SetupWorldTargets();
        RefreshUnitViews();
        Debug.Log($"Turn order: {turnOrder}");
        StartCurrentTurn();
    }

    private void OnDisable()
    {
        if (enemyTurnCoroutine != null)
        {
            StopCoroutine(enemyTurnCoroutine);
            enemyTurnCoroutine = null;
        }
    }

    private void AddBattleUnits(IEnumerable<CharacterData> characters, ICollection<BattleUnit> targetList)
    {
        foreach (var character in characters.Where(character => character != null))
        {
            targetList.Add(new BattleUnit(character));
        }
    }

    public void BasicAttack()
    {
        if (isBattleOver)
        {
            SetEnemyTargetButtonsInteractable(false);
            return;
        }

        var currentUnit = GetCurrentUnit();

        if (currentUnit == null || !currentUnit.IsAlive)
        {
            Debug.LogWarning("BattleManager cannot perform a basic attack because the current unit is missing or defeated.");
            return;
        }

        if (currentUnit.Team == CharacterTeam.Enemy)
        {
            Debug.LogWarning("Enemy turn is not controlled manually.");
            return;
        }

        currentActionMode = BattleActionMode.SelectingBasicAttackTarget;
        SetEnemyTargetButtonsInteractable(true);
        Debug.Log($"Select a target for {currentUnit.Name}'s basic attack.");
    }

    public void BasicAttackTarget(int targetBattleUnitIndex)
    {
        if (currentActionMode == BattleActionMode.SelectingSkillTarget)
        {
            SkillTarget(targetBattleUnitIndex);
            return;
        }

        if (currentActionMode != BattleActionMode.SelectingBasicAttackTarget)
        {
            Debug.LogWarning("Choose an action first.");
            return;
        }

        if (targetBattleUnitIndex < 0 || targetBattleUnitIndex >= battleUnits.Count)
        {
            Debug.LogWarning($"BattleManager received an invalid target index: {targetBattleUnitIndex}.");
            return;
        }

        var attacker = GetCurrentUnit();
        if (attacker == null || !attacker.IsAlive)
        {
            Debug.LogWarning("BattleManager cannot resolve a basic attack because the current attacker is missing or defeated.");
            currentActionMode = BattleActionMode.None;
            return;
        }

        var target = battleUnits[targetBattleUnitIndex];
        if (target == null)
        {
            Debug.LogWarning($"BattleManager received an invalid target index: {targetBattleUnitIndex}.");
            return;
        }

        if (target.Team == attacker.Team)
        {
            Debug.LogWarning("Cannot target an ally with basic attack.");
            return;
        }

        if (!target.IsAlive)
        {
            Debug.LogWarning("Cannot target a defeated unit.");
            return;
        }

        var damage = attacker.GetBasicAttackDamage();
        target.TakeDamage(damage);

        Debug.Log($"{attacker.Name} used {attacker.BasicAttackName} on {target.Name} for {damage} damage.");
        Debug.Log($"{target.Name} HP: {target.CurrentHP}/{target.MaxHP}");

        if (!target.IsAlive)
        {
            Debug.Log($"{target.Name} was defeated.");
        }

        currentActionMode = BattleActionMode.None;
        RefreshUnitViews();
        CheckBattleEnd();

        if (isBattleOver)
        {
            return;
        }

        SetEnemyTargetButtonsInteractable(false);
        EndCurrentTurn();
    }

    public void UseSkill()
    {
        if (isBattleOver)
        {
            SetEnemyTargetButtonsInteractable(false);
            return;
        }

        var currentUnit = GetCurrentUnit();

        if (currentUnit == null || !currentUnit.IsAlive)
        {
            Debug.LogWarning("BattleManager cannot use a skill because the current unit is missing or defeated.");
            return;
        }

        if (currentUnit.Team == CharacterTeam.Enemy)
        {
            Debug.LogWarning("Enemy skills are not manual yet.");
            return;
        }

        if (currentUnit.SkillTargetType == SkillTargetType.AllOpponents)
        {
            currentActionMode = BattleActionMode.None;
            SetEnemyTargetButtonsInteractable(false);

            var targets = battleUnits
                .Where(unit => unit != null && unit.Team != currentUnit.Team && unit.IsAlive)
                .ToList();

            if (targets.Count == 0)
            {
                CheckBattleEnd();
                return;
            }

            var damage = currentUnit.GetSkillDamage();
            Debug.Log($"{currentUnit.Name} used {currentUnit.SkillName}.");

            foreach (var target in targets)
            {
                target.TakeDamage(damage);
                Debug.Log($"{target.Name} took {damage} damage.");
                Debug.Log($"{target.Name} HP: {target.CurrentHP}/{target.MaxHP}");

                if (!target.IsAlive)
                {
                    Debug.Log($"{target.Name} was defeated.");
                }
            }

            RefreshUnitViews();
            CheckBattleEnd();

            if (!isBattleOver)
            {
                EndCurrentTurn();
            }

            return;
        }

        currentActionMode = BattleActionMode.SelectingSkillTarget;
        SetEnemyTargetButtonsInteractable(true);
        Debug.Log($"Select a target for {currentUnit.Name}'s skill.");
    }

    public void SkillTarget(int targetBattleUnitIndex)
    {
        if (currentActionMode != BattleActionMode.SelectingSkillTarget)
        {
            Debug.LogWarning("Choose a skill first.");
            return;
        }

        if (targetBattleUnitIndex < 0 || targetBattleUnitIndex >= battleUnits.Count)
        {
            Debug.LogWarning($"BattleManager received an invalid target index: {targetBattleUnitIndex}.");
            return;
        }

        var attacker = GetCurrentUnit();
        if (attacker == null || !attacker.IsAlive)
        {
            Debug.LogWarning("BattleManager cannot resolve a skill because the current unit is missing or defeated.");
            currentActionMode = BattleActionMode.None;
            SetEnemyTargetButtonsInteractable(false);
            return;
        }

        var target = battleUnits[targetBattleUnitIndex];
        if (target == null)
        {
            Debug.LogWarning($"BattleManager received an invalid target index: {targetBattleUnitIndex}.");
            return;
        }

        if (!target.IsAlive)
        {
            Debug.LogWarning("Cannot target a defeated unit.");
            return;
        }

        if (target.Team == attacker.Team)
        {
            Debug.LogWarning("Cannot target an ally with a skill.");
            return;
        }

        var damage = attacker.GetSkillDamage();
        target.TakeDamage(damage);

        Debug.Log($"{attacker.Name} used {attacker.SkillName} on {target.Name} for {damage} damage.");
        Debug.Log($"{target.Name} HP: {target.CurrentHP}/{target.MaxHP}");

        if (!target.IsAlive)
        {
            Debug.Log($"{target.Name} was defeated.");
        }

        currentActionMode = BattleActionMode.None;
        SetEnemyTargetButtonsInteractable(false);
        RefreshUnitViews();
        CheckBattleEnd();

        if (!isBattleOver)
        {
            EndCurrentTurn();
        }
    }

    public void OnWorldTargetClicked(int targetBattleUnitIndex)
    {
        if (isBattleOver)
        {
            return;
        }

        if (currentActionMode == BattleActionMode.SelectingBasicAttackTarget)
        {
            BasicAttackTarget(targetBattleUnitIndex);
            return;
        }

        if (currentActionMode == BattleActionMode.SelectingSkillTarget)
        {
            SkillTarget(targetBattleUnitIndex);
            return;
        }

        Debug.LogWarning("Choose an action first.");
    }

    private void BindUnitViews()
    {
        if (playerUnitViews.Count < playerBattleUnits.Count)
        {
            Debug.LogWarning($"BattleManager has fewer player unit views ({playerUnitViews.Count}) than player units ({playerBattleUnits.Count}).");
        }

        if (enemyUnitViews.Count < enemyBattleUnits.Count)
        {
            Debug.LogWarning($"BattleManager has fewer enemy unit views ({enemyUnitViews.Count}) than enemy units ({enemyBattleUnits.Count}).");
        }

        BindViewGroup(playerUnitViews, playerBattleUnits);
        BindViewGroup(enemyUnitViews, enemyBattleUnits);
    }

    private void SetupTargetButtons()
    {
        enemyTargetButtonIndices.Clear();

        foreach (var button in enemyTargetButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.Setup(this, -1);
            button.SetTargetAvailable(false);
        }

        var livingEnemyUnits = enemyBattleUnits.Where(unit => unit != null && unit.IsAlive).ToList();
        if (enemyTargetButtons.Count < livingEnemyUnits.Count)
        {
            Debug.LogWarning($"BattleManager has fewer enemy target buttons ({enemyTargetButtons.Count}) than living enemy units ({livingEnemyUnits.Count}).");
        }

        var setupCount = Mathf.Min(enemyTargetButtons.Count, livingEnemyUnits.Count);
        for (var index = 0; index < setupCount; index++)
        {
            var button = enemyTargetButtons[index];
            if (button == null)
            {
                continue;
            }

            var battleUnitIndex = battleUnits.IndexOf(livingEnemyUnits[index]);
            if (battleUnitIndex < 0)
            {
                Debug.LogWarning($"BattleManager could not find a battle unit index for enemy unit {livingEnemyUnits[index].Name}.");
                continue;
            }

            button.Setup(this, battleUnitIndex);
            button.SetTargetAvailable(false);
            enemyTargetButtonIndices[button] = battleUnitIndex;
        }
    }

    private void SetupWorldTargets()
    {
        foreach (var worldTarget in enemyWorldTargets)
        {
            if (worldTarget == null)
            {
                continue;
            }

            worldTarget.Setup(this, -1);
        }

        if (enemyWorldTargets.Count < enemyBattleUnits.Count)
        {
            Debug.LogWarning($"BattleManager has fewer enemy world targets ({enemyWorldTargets.Count}) than enemy units ({enemyBattleUnits.Count}).");
        }

        var setupCount = Mathf.Min(enemyWorldTargets.Count, enemyBattleUnits.Count);
        for (var index = 0; index < setupCount; index++)
        {
            var worldTarget = enemyWorldTargets[index];
            if (worldTarget == null)
            {
                continue;
            }

            var enemyUnit = enemyBattleUnits[index];
            var battleUnitIndex = battleUnits.IndexOf(enemyUnit);
            if (battleUnitIndex < 0)
            {
                Debug.LogWarning($"BattleManager could not find a battle unit index for enemy unit {enemyUnit.Name}.");
                continue;
            }

            worldTarget.Setup(this, battleUnitIndex);
        }
    }

    private void SetEnemyTargetButtonsInteractable(bool value)
    {
        foreach (var button in enemyTargetButtons)
        {
            if (button == null)
            {
                continue;
            }

            if (!value)
            {
                button.SetTargetAvailable(false);
                continue;
            }

            if (!enemyTargetButtonIndices.TryGetValue(button, out var battleUnitIndex) ||
                battleUnitIndex < 0 ||
                battleUnitIndex >= battleUnits.Count)
            {
                button.SetTargetAvailable(false);
                continue;
            }

            var targetUnit = battleUnits[battleUnitIndex];
            button.SetTargetAvailable(!isBattleOver && targetUnit != null && targetUnit.IsAlive);
        }
    }

    private void BindViewGroup(IReadOnlyList<BattleUnitView> views, IReadOnlyList<BattleUnit> units)
    {
        var bindCount = Mathf.Min(views.Count, units.Count);
        for (var index = 0; index < bindCount; index++)
        {
            var view = views[index];
            if (view == null)
            {
                continue;
            }

            view.Bind(units[index]);
        }
    }

    private void RefreshUnitViews()
    {
        RefreshViewGroup(playerUnitViews);
        RefreshViewGroup(enemyUnitViews);
    }

    private void RefreshViewGroup(IEnumerable<BattleUnitView> views)
    {
        foreach (var view in views)
        {
            if (view == null)
            {
                continue;
            }

            view.Refresh();
        }
    }

    private void UpdateTurnIndicator(string message)
    {
        if (turnIndicatorText != null)
        {
            turnIndicatorText.text = message;
        }

        if (!string.IsNullOrWhiteSpace(message) && message.EndsWith("'s Turn"))
        {
            Debug.Log(message);
        }
    }

    private BattleUnit GetCurrentUnit()
    {
        if (battleUnits.Count == 0)
        {
            return null;
        }

        currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, battleUnits.Count - 1);
        return battleUnits[currentTurnIndex];
    }

    public int GetBattleUnitIndexByName(string unitName)
    {
        if (string.IsNullOrWhiteSpace(unitName))
        {
            return -1;
        }

        for (var index = 0; index < battleUnits.Count; index++)
        {
            var unit = battleUnits[index];
            if (unit != null && unit.Name == unitName)
            {
                return index;
            }
        }

        return -1;
    }

    private void StartCurrentTurn()
    {
        if (isBattleOver)
        {
            StopEnemyTurnRoutine();
            SetEnemyTargetButtonsInteractable(false);
            return;
        }

        if (battleUnits.Count == 0)
        {
            StopEnemyTurnRoutine();
            Debug.LogWarning("BattleManager cannot start a turn because there are no battle units.");
            return;
        }

        if (!battleUnits.Any(unit => unit.IsAlive))
        {
            StopEnemyTurnRoutine();
            Debug.LogWarning("BattleManager cannot start a turn because no battle units are alive.");
            CheckBattleEnd();
            return;
        }

        StopEnemyTurnRoutine();
        currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, battleUnits.Count - 1);
        currentActionMode = BattleActionMode.None;
        SetEnemyTargetButtonsInteractable(false);

        for (var checkedCount = 0; checkedCount < battleUnits.Count; checkedCount++)
        {
            var currentUnit = battleUnits[currentTurnIndex];
            if (currentUnit != null && currentUnit.IsAlive)
            {
                UpdateTurnIndicator($"{currentUnit.Name}'s Turn");

                if (currentUnit.Team == CharacterTeam.Enemy)
                {
                    enemyTurnCoroutine = StartCoroutine(EnemyTurnRoutine(currentUnit));
                }

                return;
            }

            currentTurnIndex = (currentTurnIndex + 1) % battleUnits.Count;
        }

        Debug.LogWarning("BattleManager cannot start a turn because no battle units are alive.");
        CheckBattleEnd();
    }

    public void EndCurrentTurn()
    {
        if (isBattleOver)
        {
            StopEnemyTurnRoutine();
            SetEnemyTargetButtonsInteractable(false);
            return;
        }

        if (battleUnits.Count == 0)
        {
            Debug.LogWarning("BattleManager cannot end a turn because there are no battle units.");
            return;
        }

        if (enemyTurnCoroutine != null)
        {
            SetEnemyTargetButtonsInteractable(false);
            Debug.LogWarning("BattleManager is already resolving an enemy turn.");
            return;
        }

        currentTurnIndex = (currentTurnIndex + 1) % battleUnits.Count;
        StartCurrentTurn();
    }

    private IEnumerator EnemyTurnRoutine(BattleUnit enemy)
    {
        yield return new WaitForSeconds(0.7f);

        enemyTurnCoroutine = null;

        if (isBattleOver || enemy == null || !enemy.IsAlive)
        {
            yield break;
        }

        var currentUnit = GetCurrentUnit();
        if (currentUnit != enemy)
        {
            yield break;
        }

        PerformEnemyTurn(enemy);
    }

    private void PerformEnemyTurn(BattleUnit enemy)
    {
        if (enemy == null || !enemy.IsAlive || isBattleOver)
        {
            return;
        }

        SetEnemyTargetButtonsInteractable(false);

        if (enemy.SkillTargetType == SkillTargetType.AllOpponents && Random.value < 0.3f)
        {
            PerformEnemySkill(enemy);
            return;
        }

        PerformEnemyBasicAttack(enemy);
    }

    private void PerformEnemyBasicAttack(BattleUnit enemy)
    {
        if (enemy == null || !enemy.IsAlive || isBattleOver)
        {
            return;
        }

        SetEnemyTargetButtonsInteractable(false);

        var target = battleUnits.FirstOrDefault(unit =>
            unit != null &&
            unit.Team == CharacterTeam.Player &&
            unit.IsAlive);

        if (target == null)
        {
            CheckBattleEnd();
            return;
        }

        var damage = enemy.GetBasicAttackDamage();
        target.TakeDamage(damage);

        Debug.Log($"{enemy.Name} used {enemy.BasicAttackName} on {target.Name} for {damage} damage.");
        Debug.Log($"{target.Name} HP: {target.CurrentHP}/{target.MaxHP}");

        if (!target.IsAlive)
        {
            Debug.Log($"{target.Name} was defeated.");
        }

        RefreshUnitViews();
        CheckBattleEnd();

        if (!isBattleOver)
        {
            EndCurrentTurn();
        }
    }

    private void PerformEnemySkill(BattleUnit enemy)
    {
        if (enemy == null || !enemy.IsAlive || isBattleOver)
        {
            return;
        }

        SetEnemyTargetButtonsInteractable(false);

        if (enemy.SkillTargetType == SkillTargetType.SingleTarget)
        {
            PerformEnemyBasicAttack(enemy);
            return;
        }

        var targets = battleUnits
            .Where(unit => unit != null && unit.Team == CharacterTeam.Player && unit.IsAlive)
            .ToList();

        if (targets.Count == 0)
        {
            CheckBattleEnd();
            return;
        }

        var damage = enemy.GetSkillDamage();
        Debug.Log($"{enemy.Name} used {enemy.SkillName}.");

        foreach (var target in targets)
        {
            target.TakeDamage(damage);
            Debug.Log($"{target.Name} took {damage} damage.");
            Debug.Log($"{target.Name} HP: {target.CurrentHP}/{target.MaxHP}");

            if (!target.IsAlive)
            {
                Debug.Log($"{target.Name} was defeated.");
            }
        }

        RefreshUnitViews();
        CheckBattleEnd();

        if (!isBattleOver)
        {
            EndCurrentTurn();
        }
    }

    private bool HasLivingUnits(CharacterTeam team)
    {
        return battleUnits.Any(unit => unit != null && unit.Team == team && unit.IsAlive);
    }

    private void CheckBattleEnd()
    {
        if (isBattleOver)
        {
            StopEnemyTurnRoutine();
            SetEnemyTargetButtonsInteractable(false);
            return;
        }

        if (!HasLivingUnits(CharacterTeam.Enemy))
        {
            isBattleOver = true;
            currentActionMode = BattleActionMode.None;
            StopEnemyTurnRoutine();
            Debug.Log("Victory!");
            ShowBattleResult("Victory!");
            return;
        }

        if (!HasLivingUnits(CharacterTeam.Player))
        {
            isBattleOver = true;
            currentActionMode = BattleActionMode.None;
            StopEnemyTurnRoutine();
            Debug.Log("Defeat!");
            ShowBattleResult("Defeat!");
        }
    }

    private void ShowBattleResult(string resultMessage)
    {
        SetEnemyTargetButtonsInteractable(false);
        UpdateTurnIndicator("Battle Over");

        if (battleResultPanel != null)
        {
            battleResultPanel.SetActive(true);
        }

        if (battleResultText != null)
        {
            battleResultText.text = resultMessage;
        }
    }

    public void RestartBattle()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void StopEnemyTurnRoutine()
    {
        if (enemyTurnCoroutine == null)
        {
            return;
        }

        StopCoroutine(enemyTurnCoroutine);
        enemyTurnCoroutine = null;
    }
}
