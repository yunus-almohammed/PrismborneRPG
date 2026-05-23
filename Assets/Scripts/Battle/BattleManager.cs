using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private List<CharacterData> playerCharacters = new();
    [SerializeField] private List<CharacterData> enemyCharacters = new();

    private readonly List<BattleUnit> battleUnits = new();
    private int currentTurnIndex;

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
