using UnityEngine;
using UnityEngine.EventSystems;

public class WorldTarget3D : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject selectionMarker;

    private BattleManager battleManager;
    private int battleUnitIndex = -1;
    private Collider worldCollider;
    private bool hasWarnedMissingCollider;

    private void Awake()
    {
        SetMarkerVisible(false);
    }

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

    public void SetClickable(bool value)
    {
        if (worldCollider == null)
        {
            worldCollider = GetComponent<Collider>();
        }

        if (worldCollider == null)
        {
            if (!hasWarnedMissingCollider)
            {
                Debug.LogWarning("WorldTarget3D could not find a Collider on this GameObject.");
                hasWarnedMissingCollider = true;
            }

            return;
        }

        worldCollider.enabled = value;
    }

    public void SetMarkerVisible(bool value)
    {
        if (selectionMarker != null)
        {
            selectionMarker.SetActive(value);
        }
    }
}
