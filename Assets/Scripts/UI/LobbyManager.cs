using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Player Info")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerLevelText;

    [Header("Hero Slots")]
    [SerializeField] private Button[] heroSlotButtons;

    [Header("Battle")]
    [SerializeField] private Button battleButton;

    [Header("Bottom Nav")]
    [SerializeField] private Button summonButton;
    [SerializeField] private Button heroesButton;
    [SerializeField] private Button eventsButton;
    [SerializeField] private Button storeButton;

    private const string DefaultPlayerName = "Player";
    private const int DefaultPlayerLevel = 1;

    private void Start()
    {
        RefreshPlayerInfo();
        BindButtons();
    }

    private void RefreshPlayerInfo()
    {
        if (playerNameText != null)
            playerNameText.text = DefaultPlayerName;

        if (playerLevelText != null)
            playerLevelText.text = $"Level {DefaultPlayerLevel}";
    }

    private void BindButtons()
    {
        if (battleButton != null)
            battleButton.onClick.AddListener(OnBattleClicked);

        if (summonButton != null)
            summonButton.onClick.AddListener(() => Debug.Log("Summon"));

        if (heroesButton != null)
            heroesButton.onClick.AddListener(() => Debug.Log("Heroes"));

        if (eventsButton != null)
            eventsButton.onClick.AddListener(() => Debug.Log("Events"));

        if (storeButton != null)
            storeButton.onClick.AddListener(() => Debug.Log("Store"));

        if (heroSlotButtons == null)
            return;

        for (var i = 0; i < heroSlotButtons.Length; i++)
        {
            var slot = i;
            heroSlotButtons[slot]?.onClick.AddListener(() => Debug.Log($"Hero slot {slot + 1}"));
        }
    }

    private void OnBattleClicked()
    {
        SceneManager.LoadScene("BattleScene3D");
    }
}
