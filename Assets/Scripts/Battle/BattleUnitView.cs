using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUnitView : MonoBehaviour
{
    public BattleUnit boundUnit;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI hpText;
    public Image boxImage;

    public Color playerColor = new Color(0.3f, 0.6f, 1f, 1f);
    public Color enemyColor = new Color(1f, 0.4f, 0.4f, 1f);

    public void Bind(BattleUnit unit)
    {
        boundUnit = unit;

        var data = unit?.GetData();
        if (unit == null || data == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = data.characterName;
        }

        if (hpText != null)
        {
            hpText.text = $"{unit.CurrentHP} / {unit.MaxHP} HP";
        }

        if (boxImage != null)
        {
            boxImage.color = data.team == CharacterTeam.Player ? playerColor : enemyColor;
        }
    }

    public void Refresh()
    {
        var data = boundUnit?.GetData();
        if (boundUnit == null || data == null)
        {
            return;
        }

        if (nameText != null)
        {
            nameText.text = data.characterName;
        }

        if (hpText != null)
        {
            hpText.text = $"{boundUnit.CurrentHP} / {boundUnit.MaxHP} HP";
        }

        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        var isDefeated = !boundUnit.IsAlive;
        var boxAlpha = isDefeated ? 0.35f : 1f;
        var textAlpha = isDefeated ? 0.45f : 1f;

        if (boxImage != null)
        {
            var color = boxImage.color;
            color.a = boxAlpha;
            boxImage.color = color;
        }

        if (nameText != null)
        {
            var color = nameText.color;
            color.a = textAlpha;
            nameText.color = color;
        }

        if (hpText != null)
        {
            var color = hpText.color;
            color.a = textAlpha;
            hpText.color = color;
        }
    }
}
