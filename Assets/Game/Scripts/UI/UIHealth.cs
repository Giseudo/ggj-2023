using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Combat;
using TMPro;
using DG.Tweening;

public class UIHealth : MonoBehaviour
{
    [SerializeField]
    private RectMask2D _heartMask;

    [SerializeField]
    private RawImage _glowImage; 

    private RectTransform _rect;
    private Damageable _damageable;
    private Tween _hurtTween;
    private Tween _glowTween;
    private bool _isGlowing;
    public float Height => _heartMask.rectTransform.rect.height * _rect.localScale.x;

    public void Start()
    {
        TryGetComponent<RectTransform>(out _rect);

        GameManager.MainTree.TryGetComponent<Damageable>(out _damageable);

        if (_damageable == null) return;

        _damageable.hurted += OnHurt;
    }

    public void OnDestroy()
    {
        if (_damageable == null) return;

        _damageable.hurted -= OnHurt;
    }

    public void OnHurt(Damageable damageable)
    {
        _hurtTween?.Kill();
        _hurtTween = _rect.DOScale(Vector3.one * 1.2f, .2f)
            .SetUpdate(true)
            .OnComplete(() => {
                float health = (float)damageable.Health;
                float maxHealth = (float)damageable.MaxHealth;
                float value = Height * (1f - (health / maxHealth));

                _heartMask.padding = new Vector4(0f, 0f, 0f, value);

                _hurtTween?.Kill();
                _hurtTween = _rect.DOScale(Vector3.one, .2f)
                    .SetUpdate(true)
                    .SetDelay(.5f);

                if (health <= maxHealth / 2)
                    Glow();
            });
        
        GameManager.MainCamera?.DOShakePosition(.25f, 1f, 20)
            .SetUpdate(true);
    }

    public void Glow()
    {
        if (_isGlowing) return;

        _isGlowing = true;

        _glowTween?.Kill();
        _glowTween = _glowImage.DOFade(.2f, .5f)
            .SetUpdate(true)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopGlow()
    {
        _isGlowing = false;

        _glowTween?.Kill();
        _glowTween = _glowImage.DOFade(0f, .5f)
            .SetUpdate(true);
    }
}
