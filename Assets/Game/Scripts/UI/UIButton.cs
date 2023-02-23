using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.UI
{
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private float _hoverScale = 1.2f;

        private Button _button;
        private RectTransform _rect;
        private Vector3 _initialScale;

        public Button Button => _button;
        public RectTransform Rect => _rect;
        public Action clicked = delegate { };
        public Action entered = delegate { };
        public Action exited = delegate { };

        public void Awake()
        {
            TryGetComponent<Button>(out _button);
            TryGetComponent<RectTransform>(out _rect);

            _initialScale = _rect.localScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            clicked.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            entered.Invoke();

            _rect.DOScale(Vector3.one * _hoverScale, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            exited.Invoke();

            _rect.DOScale(_initialScale, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
        }
    }
}