using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class UIRootActions : MonoBehaviour
{
    [SerializeField]
    public Button _addButton;

    [SerializeField]
    public Button _upgradeButton;

    [SerializeField]
    public Button _killButton;

    [SerializeField]
    public Button _splitButton;

    [SerializeField]
    public Button _targetButton;

    private RectTransform _rect;
    private bool _isOpened = false;

    public RectTransform Rect => _rect;
    public bool IsOpened => _isOpened;

    public Action opened = delegate { };
    public Action closed = delegate { };

    public Button AddButton => _addButton;
    public Button KillButton => _killButton;
    public Button SplitButton => _splitButton;

    private Tween _tween;

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);

        if (_rect == null) return;

        _rect.localScale = Vector3.zero;
    }

    public void Show()
    {
        if (_rect == null) return;

        _tween?.Kill();
        _tween = _rect.DOScale(Vector3.one, .5f)
            .SetUpdate(true)
            .SetEase(Ease.OutExpo);

        _isOpened = true;
        opened.Invoke();
    }

    public void Hide()
    {
        if (_rect == null) return;

        _tween?.Kill();
        _tween = _rect.DOScale(Vector3.zero, .2f)
            .SetUpdate(true)
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

    public void ShowAddButton() { }
    public void HideAddButton() { }
    public void ShowUpgradeButton() { }
    public void HideUpgradeButton() { }
    public void ShowKillButton() { }
    public void HideKillButton() { }
    public void ShowTargetButton() { }
    public void HideTargetButton() { }

    public void ShowButton(Button button) { }
    public void HideButton(Button button) { }
}
