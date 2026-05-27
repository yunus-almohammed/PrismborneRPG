using UnityEngine;
using System.Collections;

public class WorldUnitView3D : MonoBehaviour
{
    [SerializeField] private GameObject unitBody;
    [SerializeField] private GameObject hpBarRoot;
    [SerializeField] private Transform hpBarFill;
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private float fullHpBarWidth = 2.5f;

    private static readonly Color HeroHpColor = new Color(0.18f, 0.88f, 0.22f);
    private static readonly Color EnemyHpColor = new Color(0.9f, 0.12f, 0.12f);

    private BattleUnit boundUnit;
    private Vector3 originalHpBarFillLocalPosition;
    private bool hasOriginalHpBarFillLocalPosition;
    private Color originalBodyColor = Color.white;
    private bool hasOriginalBodyColor;
    private Renderer hpBarFillRenderer;
    private Camera mainCamera;

    public void Bind(BattleUnit unit)
    {
        boundUnit = unit;
        mainCamera = Camera.main;

        if (unitBody == null)
        {
            unitBody = gameObject;
        }

        if (bodyRenderer == null && unitBody != null)
        {
            bodyRenderer = unitBody.GetComponent<Renderer>();
        }

        if (bodyRenderer != null && bodyRenderer.material != null)
        {
            originalBodyColor = bodyRenderer.material.color;
            hasOriginalBodyColor = true;
        }

        if (hpBarFill != null)
        {
            originalHpBarFillLocalPosition = hpBarFill.localPosition;
            hasOriginalHpBarFillLocalPosition = true;
            hpBarFillRenderer = hpBarFill.GetComponent<Renderer>();
        }

        if (hpBarFillRenderer != null && unit != null)
        {
            hpBarFillRenderer.material.color = unit.Team == CharacterTeam.Player
                ? HeroHpColor
                : EnemyHpColor;
        }

        Refresh();
    }

    private void LateUpdate()
    {
        if (hpBarRoot == null) return;
        var cam = mainCamera != null ? mainCamera : Camera.main;
        if (cam != null)
        {
            hpBarRoot.transform.rotation = cam.transform.rotation;
        }
    }

    public void Refresh()
    {
        if (boundUnit == null)
        {
            return;
        }

        var isAlive = boundUnit.IsAlive;

        if (unitBody != null)
        {
            unitBody.SetActive(isAlive);
        }

        if (hpBarRoot != null)
        {
            hpBarRoot.SetActive(isAlive);
        }
        else if (hpBarFill != null)
        {
            hpBarFill.gameObject.SetActive(isAlive);
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

    public IEnumerator PlayHitFlash()
    {
        if (bodyRenderer == null || bodyRenderer.material == null)
        {
            yield break;
        }

        if (!hasOriginalBodyColor)
        {
            originalBodyColor = bodyRenderer.material.color;
            hasOriginalBodyColor = true;
        }

        bodyRenderer.material.color = Color.white;
        yield return new WaitForSeconds(0.15f);
        bodyRenderer.material.color = originalBodyColor;
    }

    public IEnumerator PlayAttackMoveToward(Vector3 targetPosition)
    {
        var originalPosition = transform.position;
        var attackPosition = Vector3.Lerp(originalPosition, targetPosition, 0.2f);
        const float moveDuration = 0.15f;

        var elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(originalPosition, attackPosition, t);
            yield return null;
        }

        transform.position = attackPosition;
        yield return new WaitForSeconds(0.03f);

        elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(attackPosition, originalPosition, t);
            yield return null;
        }

        transform.position = originalPosition;
    }
}
