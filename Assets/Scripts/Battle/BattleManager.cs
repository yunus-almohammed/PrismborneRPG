using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private enum BattleActionMode
    {
        None,
        SelectingBasicAttackTarget
    }

    [SerializeField] private List<CharacterData> playerCharacters = new();
    [SerializeField] private List<CharacterData> enemyCharacters = new();
    [SerializeField] private List<BattleUnitView> playerUnitViews = new();
    [SerializeField] private List<BattleUnitView> enemyUnitViews = new();
    [SerializeField] private List<TargetButton> enemyTargetButtons = new();

    private readonly List<BattleUnit> battleUnits = new();
    private readonly List<BattleUnit> playerBattleUnits = new();
    private readonly List<BattleUnit> enemyBattleUnits = new();
    private readonly HashSet<TargetButton> configuredEnemyTargetButtons = new();
    private int currentTurnIndex;
    private BattleActionMode currentActionMode = BattleActionMode.None;

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
        RefreshUnitViews();
        Debug.Log($"Turn order: {turnOrder}");
        StartCurrentTurn();
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
        SetEnemyTargetButtonsInteractable(false);
        RefreshUnitViews();
        EndCurrentTurn();
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
        configuredEnemyTargetButtons.Clear();

        foreach (var button in enemyTargetButtons)
        {
            if (button == null)
            {
                continue;
            }

            button.Setup(this, -1);
            button.SetInteractable(false);
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
            button.SetInteractable(false);
            configuredEnemyTargetButtons.Add(button);
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
                button.SetInteractable(false);
                continue;
            }

            button.SetInteractable(configuredEnemyTargetButtons.Contains(button));
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
        if (battleUnits.Count == 0)
        {
            Debug.LogWarning("BattleManager cannot start a turn because there are no battle units.");
            return;
        }

        if (!battleUnits.Any(unit => unit.IsAlive))
        {
            Debug.LogWarning("BattleManager cannot start a turn because no battle units are alive.");
            return;
        }

        currentTurnIndex = Mathf.Clamp(currentTurnIndex, 0, battleUnits.Count - 1);
        currentActionMode = BattleActionMode.None;
        SetEnemyTargetButtonsInteractable(false);

        var currentUnit = battleUnits[currentTurnIndex];

        if (!currentUnit.IsAlive)
        {
            EndCurrentTurn();
            return;
        }

        Debug.Log($"{currentUnit.Name}'s turn");
    }

    public void EndCurrentTurn()
    {
        if (battleUnits.Count == 0)
        {
            Debug.LogWarning("BattleManager cannot end a turn because there are no battle units.");
            return;
        }

        currentTurnIndex = (currentTurnIndex + 1) % battleUnits.Count;
        StartCurrentTurn();
    }
}
