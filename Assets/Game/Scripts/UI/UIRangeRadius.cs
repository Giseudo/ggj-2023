using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;
using DG.Tweening;
using Freya;

public class UIRangeRadius : MonoBehaviour
{
    [SerializeField]
    private Disc _innerDisc;

    [SerializeField]
    private Disc _outerDisc;

    private Tween _thicknessTween;
    private Tween _dashTween;

    private float _time;

    public void SetRadius(float value)
    {
        DOTween.To(
            () => _outerDisc.Radius,
            x => {
                _outerDisc.Radius = x;
                _outerDisc.Thickness = value > 0f ? 0.25f : 0f;
                _innerDisc.Radius = Mathf.Max(0f, x - 0.5f);
            }, value, 1f
        )
            .SetEase(Ease.OutExpo)
            .SetUpdate(true);
    }
}
