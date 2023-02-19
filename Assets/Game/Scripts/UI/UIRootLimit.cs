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
    private Tween _tween;
    private float _lastTextChangeTime;

    public void SetText(string value)
    {
        _lastTextChangeTime = Time.unscaledTime;
        _text.text = value;

        if (!_isOpened) return;

        _tween = _rect.DOScale(Vector2.one * 1.5f, 1f)
            .SetUpdate(true)
            .SetEase(Ease.OutElastic)
            .OnComplete(Hide);
    }

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);
    }

    public void Hide()
    {
        if (_lastTextChangeTime + 0.5f > Time.unscaledTime) return;

        _isOpened = false;
        _tween = _rect.DOScale(Vector3.zero, .3f)
            .SetUpdate(true);
    }

    public void Show()
    {
        _isOpened = true;
        _tween = _rect.DOScale(Vector3.one, .3f)
            .SetUpdate(true);
    }
}
