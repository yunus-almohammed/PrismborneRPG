using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private const int MaxTeamSize = 6;

    public List<HeroData> selectedHeroes = new List<HeroData>();

    public bool IsTeamFull() => selectedHeroes.Count >= MaxTeamSize;

    public bool AddHeroToTeam(HeroData hero)
    {
        if (hero == null || IsTeamFull() || selectedHeroes.Contains(hero)) return false;
        selectedHeroes.Add(hero);
        return true;
    }

    public bool RemoveHeroFromTeam(HeroData hero)
    {
        return selectedHeroes.Remove(hero);
    }
}
