using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroCardUI : MonoBehaviour
{
    public TMP_Text heroNameText;
    public TMP_Text starsText;
    public Slider xpBar;
    public Image heroPortraitImage;
    public Image lockedOverlay;

    private const int MaxStars = 5;

    public void Setup(HeroData data)
    {
        if (heroNameText != null)
            heroNameText.text = data.heroName;

        if (starsText != null)
            starsText.text = $"{new string('*', data.stars)} ({data.stars}/{MaxStars})";

        if (xpBar != null)
            xpBar.value = data.xpToNextLevel > 0 ? data.currentXP / (float)data.xpToNextLevel : 0f;

        if (lockedOverlay != null)
        {
            lockedOverlay.color = new Color(0f, 0f, 0f, 0.6f);
            lockedOverlay.gameObject.SetActive(!data.isUnlocked);
        }
    }
}
