using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    public void Show(int damage)
    {
        if (damageText == null)
        {
            damageText = GetComponentInChildren<TextMeshProUGUI>();
        }

        if (damageText == null)
        {
            Destroy(gameObject);
            return;
        }

        damageText.text = "-" + damage;
        var textColor = damageText.color;
        textColor.a = 1f;
        damageText.color = textColor;
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        var rectTransform = transform as RectTransform;
        var startLocalPosition = rectTransform != null ? rectTransform.localPosition : transform.localPosition;
        var startColor = damageText.color;
        const float duration = 0.8f;
        const float moveDistance = 80f;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);

            var nextPosition = startLocalPosition + Vector3.up * (moveDistance * t);
            if (rectTransform != null)
            {
                rectTransform.localPosition = nextPosition;
            }
            else
            {
                transform.localPosition = nextPosition;
            }

            var nextColor = startColor;
            nextColor.a = Mathf.Lerp(1f, 0f, t);
            damageText.color = nextColor;

            yield return null;
        }

        Destroy(gameObject);
    }
}
