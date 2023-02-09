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
    public class UIUnitCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
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
        public void Click() => clicked.Invoke(this);
        public void Select()
        {
            selected.Invoke(this);
            _isSelected = true;
            
            DOTween.To(() => _disc.Radius, x => _disc.Radius = x, SELECTED_DISC_RADIUS, .3f);
            DOTween.To(() => _disc.Thickness, x => _disc.Thickness = x, SELECTED_DISC_THICKNESS, .3f);
            DOTween.To(() => _disc.Color, x => _disc.Color = x, _isDisabled ? new Color32(185, 46, 49, 255) : new Color32(46, 185, 132, 255), .3f);
            _thumbnail.rectTransform.DOScale(Vector3.one, .3f);
            _energyButton.Rect.DOScale(Vector3.one, .3f);
        }

        public void Deselect()
        {
            deselected.Invoke(this);
            _isSelected = false;
            
            DOTween.To(() => _disc.Radius, x => _disc.Radius = x, _initialDiscRadius, .3f);
            DOTween.To(() => _disc.Thickness, x => _disc.Thickness = x, _initialDiscThickness, .3f);
            DOTween.To(() => _disc.Color, x => _disc.Color = x, _isDisabled ? new Color32(185, 46, 49, 50) : new Color32(46, 185, 132, 50), .3f);
            _thumbnail.rectTransform.DOScale(Vector3.one * 0.7f, .3f);
            _energyButton.Rect.DOScale(Vector3.zero, .3f);
        }

        public void Disable() {
            _isDisabled = true;
            _energyButton.Disable();

            DOTween.To(() => _disc.Color, x => _disc.Color = x, _isSelected ? new Color32(185, 46, 49, 255) : new Color32(185, 46, 49, 50), .3f);
        }

        public void Enable() {
            _isDisabled = false;
            _energyButton.Enable();

            DOTween.To(() => _disc.Color, x => _disc.Color = x, _isSelected ? new Color32(46, 185, 132, 255) : new Color32(46, 185, 132, 50), .3f);
        }

        private void OnEnergyChange(int amount, Vector3 position)
        {
            if (GameManager.MainTree.EnergyAmount >= _data.RequiredEnergy)
                Enable();
            else
                Disable();
        }
    }
}