using UnityEngine;

public enum CharacterTeam
{
    Player,
    Enemy
}

[CreateAssetMenu(menuName = "RPG/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public CharacterTeam team;
    public int maxHP = 1;
    public int attack;
    public int speed = 1;
    public string basicAttackName;
    public string skillName;
    public int skillPower;

    private void OnValidate()
    {
        maxHP = Mathf.Max(1, maxHP);
        attack = Mathf.Max(0, attack);
        speed = Mathf.Max(1, speed);
        skillPower = Mathf.Max(0, skillPower);
    }
}
