using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Game.Combat;

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
    public Button TargetButton => _targetButton;
    public Button UpgradeButton => _upgradeButton;

    private Tween _tween;

    public void Awake()
    {
        TryGetComponent<RectTransform>(out _rect);

        if (_rect == null) return;

        _rect.localScale = Vector3.zero;
    }

    public void Show(RootNode node)
    {
        if (node.Unit == null)
        {
            _addButton.gameObject.SetActive(true);
            _killButton.gameObject.SetActive(false);
            _upgradeButton.gameObject.SetActive(false);
            _targetButton.gameObject.SetActive(false);
        }

        if (node.Unit != null)
        {
            _addButton.gameObject.SetActive(false);
            _killButton.gameObject.SetActive(true);
            _upgradeButton.gameObject.SetActive(true);
            _targetButton.gameObject.SetActive(false);
            // _targetButton.gameObject.SetActive(true); // TODO: only for sementinha :3
        }

        if (node.Parent == null)
        {
            _addButton.gameObject.SetActive(false);
            _killButton.gameObject.SetActive(false);
            _upgradeButton.gameObject.SetActive(true);
            _targetButton.gameObject.SetActive(false);
        }


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
        // if (_isOpened) Hide();
        // else Show();
    }
}
