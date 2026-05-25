using UnityEngine;
using UnityEngine.EventSystems;

public class WorldTarget3D : MonoBehaviour, IPointerClickHandler
{
    private BattleManager battleManager;
    private int battleUnitIndex = -1;

    public void Setup(BattleManager manager, int unitIndex)
    {
        battleManager = manager;
        battleUnitIndex = unitIndex;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (battleManager == null)
        {
            Debug.LogWarning("WorldTarget3D is missing a BattleManager reference.");
            return;
        }

        if (battleUnitIndex < 0)
        {
            Debug.LogWarning("WorldTarget3D does not have a valid battle unit index.");
            return;
        }

        battleManager.OnWorldTargetClicked(battleUnitIndex);
    }
}
