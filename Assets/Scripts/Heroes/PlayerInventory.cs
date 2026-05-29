using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<HeroData> ownedHeroes = new List<HeroData>();

    private void Awake()
    {
        HeroData[] loaded = Resources.LoadAll<HeroData>("PlayerHeroes");
        ownedHeroes = new List<HeroData>(loaded);
    }

    public void AddHero(HeroData hero)
    {
        if (hero == null || ownedHeroes.Contains(hero)) return;
        ownedHeroes.Add(hero);
    }
}
