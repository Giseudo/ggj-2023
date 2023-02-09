using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEnergyButton : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    [SerializeField]
    private Sprite _disabledSprite;

    private RectTransform _rect;
    private Image _image;
    private Sprite _defaultSprite;

    public RectTransform Rect => _rect;

    public void SetText(string value) => _text.text = value;

    public void Awake()
    {
        TryGetComponent<Image>(out _image);
        TryGetComponent<RectTransform>(out _rect);

        _defaultSprite = _image.sprite;
    }

    public void Disable()
    {
        _image.sprite = _disabledSprite;
        _text.color = new Color32(248, 18, 62, 255);
    }

    public void Enable()
    {
        _image.sprite = _defaultSprite;
        _text.color = new Color32(255, 213, 4, 255);
    }
}
