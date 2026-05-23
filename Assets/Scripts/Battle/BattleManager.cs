using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private List<CharacterData> playerCharacters = new();
    [SerializeField] private List<CharacterData> enemyCharacters = new();

    private readonly List<BattleUnit> battleUnits = new();

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

        if (battleUnits.Count == 0)
        {
            Debug.Log("Turn order: no valid battle units found.");
            return;
        }

        var turnOrder = string.Join(", ", battleUnits.Select((unit, index) =>
            $"{index + 1}. {unit.Name} ({unit.Team}) SPD {unit.Speed}"));

        Debug.Log($"Turn order: {turnOrder}");
    }

    private void AddBattleUnits(IEnumerable<CharacterData> characters)
    {
        foreach (var character in characters.Where(character => character != null))
        {
            battleUnits.Add(new BattleUnit(character));
        }
    }
}
