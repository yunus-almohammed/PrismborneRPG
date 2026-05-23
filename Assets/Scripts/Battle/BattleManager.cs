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

    private readonly List<BattleUnit> battleUnits = new();
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
        AddBattleUnits(playerCharacters);
        AddBattleUnits(enemyCharacters);

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

        Debug.Log($"Turn order: {turnOrder}");
        StartCurrentTurn();
    }

    private void AddBattleUnits(IEnumerable<CharacterData> characters)
    {
        foreach (var character in characters.Where(character => character != null))
        {
            battleUnits.Add(new BattleUnit(character));
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
            Debug.Log("Enemy actions are not manual yet.");
            return;
        }

        currentActionMode = BattleActionMode.SelectingBasicAttackTarget;
        Debug.Log($"Select a target for {currentUnit.Name}'s basic attack.");
    }

    public void BasicAttackTarget(int targetBattleUnitIndex)
    {
        if (currentActionMode != BattleActionMode.SelectingBasicAttackTarget)
        {
            Debug.LogWarning("BattleManager is not currently selecting a basic attack target.");
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
        if (target == null || !target.IsAlive)
        {
            Debug.LogWarning("BattleManager cannot resolve a basic attack because the selected target is missing or defeated.");
            return;
        }

        if (target.Team == attacker.Team)
        {
            Debug.LogWarning("BattleManager cannot resolve a basic attack against a unit on the same team.");
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
        EndCurrentTurn();
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
