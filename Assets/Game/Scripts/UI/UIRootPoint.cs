using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIRootPoint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private RectTransform _innerRect;

    [SerializeField]
    private RectTransform _outerRect;

    private RectTransform _rect;
    private Button _button;
    private bool _isOpened;

    public bool IsOpened => _isOpened;
    public RectTransform Rect => _rect;

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);
        TryGetComponent<Button>(out _button);
    }

    public void Show()
    {
        ShowInner();
        ShowOuter();

        _isOpened = true;
    }

    public void Hide()
    {
        HideOuter();
        HideInner();

        _isOpened = false;
    }

    public Tween ShowInner() => _innerRect.DOScale(Vector3.one, .3f);
    public Tween ShowOuter() => _outerRect.DOScale(Vector3.one, .3f);
    public Tween HideInner() => _innerRect.DOScale(Vector3.zero, .3f);
    public Tween HideOuter() => _outerRect.DOScale(Vector3.zero, .3f);

    public void OnPointerEnter(PointerEventData evt)
    {
        if (_innerRect.localScale == Vector3.zero) return;
        if (_isOpened) return;

        ShowOuter();
    }

    public void OnPointerExit(PointerEventData evt)
    {
        if (_isOpened) return;

        HideOuter();
    }
}
