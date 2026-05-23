using UnityEngine;

public class BattleUnit
{
    public CharacterData Data { get; private set; }
    public int CurrentHP { get; private set; }

    public string Name => Data != null ? Data.characterName : string.Empty;
    public string BasicAttackName => Data != null && !string.IsNullOrWhiteSpace(Data.basicAttackName) ? Data.basicAttackName : "Basic Attack";
    public CharacterTeam Team => Data != null ? Data.team : default;
    public int MaxHP => Data != null ? Data.maxHP : 0;
    public int Attack => Data != null ? Data.attack : 0;
    public int Speed => Data != null ? Data.speed : 0;
    public bool IsAlive => CurrentHP > 0;

    public BattleUnit(CharacterData data)
    {
        Data = data;

        if (Data == null)
        {
            Debug.LogError("BattleUnit requires a valid CharacterData reference.");
            CurrentHP = 0;
            return;
        }

        CurrentHP = Data.maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
    }

    public int GetBasicAttackDamage()
    {
        return Data != null ? Data.attack : 0;
    }

    public int GetSkillDamage()
    {
        return Data != null ? Data.skillPower : 0;
    }

    public CharacterData GetData()
    {
        return Data;
    }

    public void HealToFull()
    {
        CurrentHP = MaxHP;
    }
}
