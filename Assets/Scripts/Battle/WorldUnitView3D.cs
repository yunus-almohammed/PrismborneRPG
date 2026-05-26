using UnityEngine;

public class WorldUnitView3D : MonoBehaviour
{
    [SerializeField] private GameObject unitBody;
    [SerializeField] private Transform hpBarFill;
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private float fullHpBarWidth = 1.2f;

    private BattleUnit boundUnit;
    private Vector3 originalHpBarFillLocalPosition;
    private bool hasOriginalHpBarFillLocalPosition;

    public void Bind(BattleUnit unit)
    {
        boundUnit = unit;

        if (unitBody == null)
        {
            unitBody = gameObject;
        }

        if (bodyRenderer == null && unitBody != null)
        {
            bodyRenderer = unitBody.GetComponent<Renderer>();
        }

        if (hpBarFill != null)
        {
            originalHpBarFillLocalPosition = hpBarFill.localPosition;
            hasOriginalHpBarFillLocalPosition = true;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (boundUnit == null)
        {
            return;
        }

        if (unitBody != null)
        {
            unitBody.SetActive(boundUnit.IsAlive);
        }

        if (hpBarFill != null)
        {
            var hpPercent = boundUnit.MaxHP > 0
                ? Mathf.Clamp01((float)boundUnit.CurrentHP / boundUnit.MaxHP)
                : 0f;

            var scale = hpBarFill.localScale;
            scale.x = fullHpBarWidth * hpPercent;
            hpBarFill.localScale = scale;

            if (hasOriginalHpBarFillLocalPosition)
            {
                var position = originalHpBarFillLocalPosition;
                position.x -= (fullHpBarWidth - scale.x) * 0.5f;
                hpBarFill.localPosition = position;
            }
        }
    }
}
