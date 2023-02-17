using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using DG.Tweening;
using Freya;

public class UIRootSelector : MonoBehaviour
{
    [SerializeField]
    private Disc _disc;

    private Tween _thicknessTween;
    private Tween _dashTween;
    private float _time;

    public void Show()
    {
        _thicknessTween?.Kill();
        _thicknessTween = DOTween.To(
            () => _time,
            x => {
                _disc.Radius = x * 3f;
                _disc.Thickness = x * 0.25f;
                _time = x;
            }, 1f, 1.5f
        )
            .SetUpdate(true)
            .SetEase(Ease.OutExpo);
        
        if (_dashTween != null && _dashTween.IsPlaying())
            return;

        _dashTween = DOTween.To(
            () => _disc.DashOffset,
            x => { _disc.DashOffset += Time.unscaledDeltaTime; },
            1f, 1f
        )
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .SetLoops(-1);
    }

    public void Hide()
    {
        _thicknessTween?.Kill();
        _thicknessTween = DOTween.To(
            () => _time,
            x => {
                _disc.Radius = x * 3f;
                _disc.Thickness = x * 0.25f;
                _time = x;
            }, 0f, 1.5f
        )
            .SetUpdate(true)
            .SetEase(Ease.OutExpo);
        
        _dashTween?.Kill();
    }

}
