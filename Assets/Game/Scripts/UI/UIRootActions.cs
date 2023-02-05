using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIRootActions : MonoBehaviour
{
    [SerializeField]
    public Button _addButton;

    [SerializeField]
    public Button _killButton;

    [SerializeField]
    public Button _splitButton;

    private RectTransform _rect;
    private bool _isOpened = false;

    public RectTransform Rect => _rect;
    public bool IsOpened => _isOpened;

    public Action opened = delegate { };
    public Action closed = delegate { };

    public Button AddButton => _addButton;
    public Button KillButton => _killButton;
    public Button SplitButton => _splitButton;

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);

        if (_rect == null) return;

        _rect.localScale = Vector3.zero;
    }

    public void Show()
    {
        if (_rect == null) return;

        _rect.DOScale(Vector3.one, .5f)
            .SetEase(Ease.OutExpo);

        _isOpened = true;
        opened.Invoke();
    }

    public void Hide()
    {
        if (_rect == null) return;

        _rect.DOScale(Vector3.zero, .2f)
            .OnComplete(() => {
                _isOpened = false;
                closed.Invoke();
            });
    }

    public void Toggle()
    {
        if (_isOpened) Hide();
        else Show();
    }
}
