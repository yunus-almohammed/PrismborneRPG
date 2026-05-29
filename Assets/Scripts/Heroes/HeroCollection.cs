using System.Collections.Generic;
using UnityEngine;

public class HeroCollection : MonoBehaviour
{
    public List<HeroData> allHeroes = new List<HeroData>();
    public List<HeroData> unlockedHeroes = new List<HeroData>();

    private void Awake()
    {
        HeroData[] heroes = Resources.LoadAll<HeroData>("Heroes");
        allHeroes = new List<HeroData>(heroes);

        HeroData[] playerHeroes = Resources.LoadAll<HeroData>("PlayerHeroes");
        unlockedHeroes = new List<HeroData>(playerHeroes);
    }

    public void UnlockHero(HeroData hero)
    {
        if (hero == null || hero.isUnlocked) return;
        hero.isUnlocked = true;
        if (!unlockedHeroes.Contains(hero))
            unlockedHeroes.Add(hero);
    }

    public void AddDuplicate(HeroData hero)
    {
        if (hero == null) return;
        hero.duplicateCount++;
    }

    public void AddXP(HeroData hero, int amount)
    {
        if (hero == null || amount <= 0) return;
        hero.currentXP += amount;
        while (hero.currentXP >= hero.xpToNextLevel && hero.xpToNextLevel > 0)
        {
            hero.currentXP -= hero.xpToNextLevel;
            hero.level++;
        }
    }
}
