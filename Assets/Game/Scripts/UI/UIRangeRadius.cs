using UnityEngine;
using Shapes;
using DG.Tweening;

namespace Game.UI
{
    public class UIRangeRadius : MonoBehaviour
    {
        public const int INNER_ALPHA = 15;

        [SerializeField]
        private Disc _innerDisc;

        [SerializeField]
        private Disc _outerDisc;

        private float _time;
        private Tween _thicknessTween;
        private Tween _dashTween;
        private Color32 _initialColor;

        public Disc InnerDisc => _innerDisc;
        public Disc OuterDisc => _outerDisc;
        public Color32 InitialColor => _initialColor;

        public void Awake()
        {
            _initialColor = _outerDisc.Color;
        }

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

        public void SetColor(Color32 color)
        {
            _innerDisc.Color = new Color32(color.r, color.g, color.b, INNER_ALPHA);
            _outerDisc.Color = color;
        }
    }
}