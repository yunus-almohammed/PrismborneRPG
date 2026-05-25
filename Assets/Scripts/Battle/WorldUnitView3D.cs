using UnityEngine;

public class WorldUnitView3D : MonoBehaviour
{
    [SerializeField] private GameObject unitBody;
    [SerializeField] private Transform hpBarFill;
    [SerializeField] private Renderer bodyRenderer;

    private BattleUnit boundUnit;

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
            scale.x = hpPercent;
            hpBarFill.localScale = scale;
        }
    }
}
