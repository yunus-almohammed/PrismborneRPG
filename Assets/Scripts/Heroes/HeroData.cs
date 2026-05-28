using UnityEngine;

public enum HeroClass { Warrior, Mage, Archer, Healer }

[CreateAssetMenu(fileName = "NewHero", menuName = "CrystalborneRPG/Hero")]
public class HeroData : ScriptableObject
{
    public string heroName;
    public HeroClass heroClass;

    public int maxHealth;
    public int attackPower;
    public int defense;
    public int speed;

    public int level = 1;
    public int currentXP;
    public int xpToNextLevel;

    [Range(1, 5)]
    public int stars = 1;
    public int duplicateCount;

    public bool isUnlocked;

    public bool isEligibleForEvolution => level >= 10 && duplicateCount >= 10;
}
