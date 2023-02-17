using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Combat;
using Shapes;
using DG.Tweening;
using Game.Core;

namespace Game.UI
{
    public class UIUnitCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        const float SELECTED_DISC_RADIUS = 56.8f;
        const float SELECTED_DISC_THICKNESS = 66f;

        [SerializeField]
        private UnitData _data;

        [SerializeField]
        private Image _thumbnail;

        [SerializeField]
        private UIEnergyButton _energyButton;

        [SerializeField]
        private Disc _disc;

        private bool _isDisabled;

        public UnitData Data => _data;

        public Action<UIUnitCard> clicked = delegate { };
        public Action<UIUnitCard> selected = delegate { };
        public Action<UIUnitCard> deselected = delegate { };

        private float _initialDiscRadius;
        private float _initialDiscThickness;
        private bool _isSelected;

        public void Awake()
        {
            _initialDiscRadius = _disc.Radius;
            _initialDiscThickness = _disc.Thickness;
            _energyButton.SetText($"{_data.RequiredEnergy}");
        }

        public void Start()
        {
            GameManager.MainTree.collectedEnergy += OnEnergyChange;
            GameManager.MainTree.consumedEnergy += OnEnergyChange;

            if (GameManager.MainTree.EnergyAmount >= _data.RequiredEnergy)
                Enable();
            else
                Disable();
        }

        public void OnDestroy()
        {
            GameManager.MainTree.collectedEnergy -= OnEnergyChange;
            GameManager.MainTree.consumedEnergy -= OnEnergyChange;
        }

        public void OnPointerClick(PointerEventData evt) => Click();
        public void OnPointerEnter(PointerEventData evt) => Select();
        public void OnPointerExit(PointerEventData evt) => Deselect();
        public void Click() => clicked.Invoke(this);
        public void Select()
        {
            selected.Invoke(this);
            _isSelected = true;

            Color color = _isDisabled ? new Color32(185, 46, 49, 255) : new Color32(46, 185, 132, 255);
            
            DOTween.To(() => _disc.Radius, x => _disc.Radius = x, SELECTED_DISC_RADIUS, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Thickness, x => _disc.Thickness = x, SELECTED_DISC_THICKNESS, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);

            _thumbnail.rectTransform.DOScale(Vector3.one, .3f).SetUpdate(true);
            _energyButton.Rect.DOScale(Vector3.one, .3f).SetUpdate(true);
        }

        public void Deselect()
        {
            deselected.Invoke(this);
            _isSelected = false;

            Color color = _isDisabled ? new Color32(185, 46, 49, 50) : new Color32(46, 185, 132, 50);
            
            DOTween.To(() => _disc.Radius, x => _disc.Radius = x, _initialDiscRadius, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Thickness, x => _disc.Thickness = x, _initialDiscThickness, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);

            _thumbnail.rectTransform.DOScale(Vector3.one * 0.7f, .3f).SetUpdate(true);
            _energyButton.Rect.DOScale(Vector3.zero, .3f).SetUpdate(true);
        }

        public void Disable() {
            _isDisabled = true;
            _energyButton.Disable();

            Color color = _isSelected ? new Color32(185, 46, 49, 255) : new Color32(185, 46, 49, 50);

            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);
        }

        public void Enable() {
            _isDisabled = false;
            _energyButton.Enable();

            Color color = _isSelected ? new Color32(46, 185, 132, 255) : new Color32(46, 185, 132, 50);

            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);
        }

        private void OnEnergyChange(int amount)
        {
            if (GameManager.MainTree.EnergyAmount >= _data.RequiredEnergy)
                Enable();
            else
                Disable();
        }
    }
}