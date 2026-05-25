using UnityEngine;
using UnityEngine.UI;

public class TargetButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private BattleUnitView unitView;

    private BattleManager battleManager;
    private int battleUnitIndex = -1;

    public void Setup(BattleManager manager, int unitIndex)
    {
        battleManager = manager;
        battleUnitIndex = unitIndex;

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => battleManager.BasicAttackTarget(battleUnitIndex));
    }

    public void SetInteractable(bool value)
    {
        SetTargetAvailable(value);
    }

    public void SetTargetAvailable(bool value)
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.interactable = value;
        }
    }
}
