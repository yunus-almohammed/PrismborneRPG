using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class HeroCardClickHandler : MonoBehaviour
{
    private HeroData _data;
    private Action<HeroData> _onClick;

    public void Init(HeroData data, Action<HeroData> onClick)
    {
        _data = data;
        _onClick = onClick;
        GetComponent<Button>().onClick.AddListener(() => _onClick?.Invoke(_data));
    }
}
