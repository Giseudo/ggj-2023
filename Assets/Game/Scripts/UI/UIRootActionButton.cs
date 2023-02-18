using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Game.UI
{
    public class UIRootActionButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private UIEnergyButton _energyButton;

        private RectTransform _rect;

        public Action clicked = delegate { };

        public UIEnergyButton EnergyButton => _energyButton;

        public void OnPointerClick(PointerEventData eventData)
        {
            clicked.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _energyButton?.Rect.DOLocalMoveY(40f, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
            _energyButton?.Rect.DOScale(Vector3.one * .5f, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _energyButton?.Rect.DOLocalMoveY(0f, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);
            _energyButton?.Rect.DOScale(Vector3.zero, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);

        }
    }
}