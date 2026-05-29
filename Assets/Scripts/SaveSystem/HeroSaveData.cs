using System.Collections.Generic;
using UnityEngine;

public static class HeroSaveData
{
    private const string UnlockedKey = "UnlockedHeroes";

    public static void SaveUnlockedHeroes(List<HeroData> allHeroes)
    {
        var names = new List<string>();
        foreach (HeroData hero in allHeroes)
        {
            if (hero.isUnlocked)
                names.Add(hero.heroName);
        }
        PlayerPrefs.SetString(UnlockedKey, string.Join(",", names));
        PlayerPrefs.Save();
    }

    public static void LoadUnlockedHeroes(List<HeroData> allHeroes)
    {
        string raw = PlayerPrefs.GetString(UnlockedKey, string.Empty);
        if (string.IsNullOrEmpty(raw)) return;

        var unlockedNames = new HashSet<string>(raw.Split(','));
        foreach (HeroData hero in allHeroes)
            hero.isUnlocked = unlockedNames.Contains(hero.heroName);
    }
}
