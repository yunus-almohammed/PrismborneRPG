using UnityEngine;

public class HeroesScreenManager : MonoBehaviour
{
    public GameObject heroCardPrefab;
    public Transform gridContent;
    public HeroCollection heroCollection;

    void Start()
    {
        PopulateGrid();
    }

    public void PopulateGrid()
    {
        foreach (Transform child in gridContent)
            Destroy(child.gameObject);

        foreach (HeroData hero in heroCollection.allHeroes)
        {
            GameObject card = Instantiate(heroCardPrefab, gridContent);
            HeroCardUI cardUI = card.GetComponent<HeroCardUI>();
            if (cardUI != null)
                cardUI.Setup(hero);

            HeroCardClickHandler clickHandler = card.GetComponent<HeroCardClickHandler>();
            if (clickHandler != null)
                clickHandler.Init(hero, OnHeroCardClicked);
        }
    }

    public void OnHeroCardClicked(HeroData data)
    {
        Debug.Log($"Hero clicked: {data.heroName}");
    }
}
