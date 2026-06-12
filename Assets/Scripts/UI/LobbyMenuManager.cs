using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject homeRoot;
    [SerializeField] private GameObject heroesScreen;

    private void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        if (homeRoot != null)
        {
            homeRoot.SetActive(true);
        }

        if (heroesScreen != null)
        {
            heroesScreen.SetActive(false);
        }
    }

    public void ShowHeroes()
    {
        if (homeRoot != null)
        {
            homeRoot.SetActive(false);
        }

        if (heroesScreen != null)
        {
            heroesScreen.SetActive(true);
        }
    }

    public void LoadBattle()
    {
        SceneManager.LoadScene("BattleScene3D");
    }

    public void StoreNotReady()
    {
        Debug.Log("Store is not ready yet.");
    }

    public void EventsNotReady()
    {
        Debug.Log("Events are not ready yet.");
    }

    public void SummonNotReady()
    {
        Debug.Log("Summon is not ready yet.");
    }
}
