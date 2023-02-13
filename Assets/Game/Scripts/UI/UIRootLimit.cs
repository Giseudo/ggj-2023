using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIRootLimit : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    private RectTransform _rect;
    private bool _isOpened;

    public TextMeshProUGUI Text => _text;
    public RectTransform Rect => _rect;

    public void SetText(string value) => _text.text = value;

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);
    }

    public void Hide()
    {
        _isOpened = false;
        _rect.DOScale(Vector3.zero, .3f);
    }

    public void Show()
    {
        _isOpened = true;
        _rect.DOScale(Vector3.one, .3f);
    }
}
