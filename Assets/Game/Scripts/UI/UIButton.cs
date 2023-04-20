using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Game.Core;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField]
        private float _hoverScale = 1.2f;

        [SerializeField]
        private AudioClip _clickClip;

        [SerializeField]
        private AudioClip _disabledClip;

        private Button _button;
        private RectTransform _rect;
        private Vector3 _initialScale;
        private bool _isPulsing;
        private bool _isDisabled;
        private Tween _pulseTween;
        private Tween _rectTween;

        public Button Button => _button;
        public RectTransform Rect => _rect;
        public Action clicked = delegate { };
        public Action entered = delegate { };
        public Action exited = delegate { };

        public void Enable() => _isDisabled = false;
        public void Disable() => _isDisabled = true;

        public void Awake()
        {
            TryGetComponent<Button>(out _button);
            TryGetComponent<RectTransform>(out _rect);

            _initialScale = _rect.localScale == Vector3.zero ? Vector3.one : _rect.localScale;
        }

        public void OnEnable()
        {
        }

        public void Pulse(bool enable)
        {
            _isPulsing = enable;

            _pulseTween?.Kill();

            if (!enable)
            {
                _pulseTween = _rect.DOScale(_initialScale, .5f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutExpo);
                return;
            }

            _pulseTween = _rect.DOScale(Vector3.one * _hoverScale, .5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _rectTween?.Kill();
            _rectTween = _rect.DOScale(Vector3.one * .95f, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_button.interactable)
            {
                SoundManager.PlaySound(_disabledClip, 0.5f);

                return;
            }

            SoundManager.PlaySound(_clickClip, 1f);

            clicked.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            entered.Invoke();

            Grow();
        }

        private void Grow()
        {
            if (_isPulsing) return;
            if (_isDisabled) return;

            _rectTween?.Kill();
            _rectTween = _rect.DOScale(Vector3.one * _hoverScale, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
        }

        private void Shrink()
        {
            if (_isPulsing) return;
            if (_isDisabled) return;

            _rectTween?.Kill();
            _rectTween = _rect.DOScale(_initialScale, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            exited.Invoke();

            Shrink();
        }

        public void OnSelect(BaseEventData eventData) => Grow();
        public void OnDeselect(BaseEventData eventData) => Shrink();
    }
}