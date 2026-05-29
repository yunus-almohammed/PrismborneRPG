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

    public void Setup(HeroData data)
    {
        heroNameText.text = data.heroName;
        starsText.text = new string('★', data.stars);

        if (data.xpToNextLevel > 0)
            xpBar.value = (float)data.currentXP / data.xpToNextLevel;
        else
            xpBar.value = 0f;

        if (lockedOverlay != null)
        {
            lockedOverlay.gameObject.SetActive(!data.isUnlocked);
            lockedOverlay.color = new Color(0f, 0f, 0f, 0.6f);
        }
    }
}
