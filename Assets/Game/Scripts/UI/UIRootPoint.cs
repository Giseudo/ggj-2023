using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIRootPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField]
    private RectTransform _innerRect;

    [SerializeField]
    private RectTransform _outerRect;

    private bool _isOpened;
    private bool _isEnabled;
    private bool _isPulsing;
    private RectTransform _rect;
    private Button _button;
    private Tween _pulseTween;
    private Tween _innerTween;
    private Tween _outerTween;

    public bool IsOpened => _isOpened;
    public bool IsEnabled => _isEnabled;
    public bool IsPulsing => _isPulsing;
    public RectTransform Rect => _rect;
    public Action clicked = delegate { };

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);
        TryGetComponent<Button>(out _button);
    }

    public void Show()
    {
        if (!_isEnabled) return;

        if (!_isPulsing)
        {
            ShowInner();
            ShowOuter();
        }

        _isOpened = true;
    }

    public void OnEnable()
    {
        Enable();
    }

    public void Hide()
    {
        if (!_isPulsing)
        {
            HideOuter();
            HideInner();
        }

        _isOpened = false;
    }

    public void Toggle()
    {
        if (_isOpened) Hide();
        else Show();
    }

    public void Enable()
    {
        _isEnabled = true;
    }

    public void Disable()
    {
        _isEnabled = false;
    }

    public Tween ShowInner() 
    {
        _innerTween?.Kill();
        _innerTween = _innerRect.DOScale(Vector3.one, .3f).SetUpdate(true);

        return _innerTween;
    }

    public Tween ShowOuter()
    {
        _outerTween?.Kill();
        _outerTween = _outerRect.DOScale(Vector3.one, .3f).SetUpdate(true);

        return _outerTween;
    }

    public Tween HideInner()
    {
        _innerTween?.Kill();
        _innerTween = _innerRect.DOScale(Vector3.zero, .3f).SetUpdate(true);

        return _innerTween;
    }

    public Tween HideOuter()
    {
        _outerTween?.Kill();
        _outerTween = _outerRect.DOScale(Vector3.zero, .3f).SetUpdate(true);

        return _outerTween;
    }

    public void OnPointerEnter(PointerEventData evt)
    {
        if (_isPulsing) return;
        if (!_isEnabled) return;
        if (_innerRect.localScale == Vector3.zero) return;
        if (_isOpened) return;

        ShowOuter();
    }

    public void OnPointerClick(PointerEventData evt) => clicked.Invoke();

    public void OnPointerExit(PointerEventData evt)
    {
        if (_isPulsing) return;
        if (!_isEnabled) return;
        if (_isOpened) return;

        HideOuter();
    }

    public void Pulse(bool enable = true)
    {
        _isPulsing = enable;

        if (!enable)
        {
            _pulseTween?.Kill();
            HideOuter();
            return;
        }

        ShowOuter()
            .OnComplete(() => {
                ShowInner();

                _pulseTween = DOTween.To(() => 0f, x => {
                    float t = 1f + Mathf.Abs((x - 0.5f) * 2f) * .25f;
                    _outerRect.localScale = Vector3.one * t;
                }, 1f, 1f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1);
            });
    }
}
