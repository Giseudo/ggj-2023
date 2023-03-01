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
        const float SELECTED_DISC_RADIUS = 76.5f;
        const float SELECTED_DISC_THICKNESS = 27.2f;
        const int DISABLED_ALPHA = 100;

        [SerializeField]
        private UnitData _data;

        [SerializeField]
        private bool _isAvailable;

        [SerializeField]
        private Image _thumbnail;

        [SerializeField]
        private UIEnergyButton _energyButton;

        [SerializeField]
        private Disc _disc;

        public UnitData Data => _data;

        public Action<UIUnitCard> clicked = delegate { };
        public Action<UIUnitCard> selected = delegate { };
        public Action<UIUnitCard> deselected = delegate { };

        private float _initialDiscRadius;
        private float _initialDiscThickness;
        private bool _isSelected;
        private bool _isDisabled;

        public void Awake()
        {
            _initialDiscRadius = _disc.Radius;
            _initialDiscThickness = _disc.Thickness;
            _energyButton.SetText($"{_data.RequiredEnergy}");
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;

            GameManager.MainTree.collectedEnergy += OnEnergyChange;
            GameManager.MainTree.consumedEnergy += OnEnergyChange;

            if (GameManager.MainTree.EnergyAmount >= _data.RequiredEnergy)
                Enable();
            else
                Disable();

            if (!_isAvailable)
                Disable();
        }

        private void OnLevelLoad(int level)
        {
            GameManager.MainTree.collectedEnergy += OnEnergyChange;
            GameManager.MainTree.consumedEnergy += OnEnergyChange;
        }

        public void OnPointerClick(PointerEventData evt) => Click();
        public void OnPointerEnter(PointerEventData evt) => Select();
        public void OnPointerExit(PointerEventData evt) => Deselect();
        public void Click()
        {
            if (_isDisabled) return;

            clicked.Invoke(this);
        }

        public void Select()
        {
            if (!_isAvailable) return;

            selected.Invoke(this);
            _isSelected = true;

            Color color = _isDisabled ? new Color32(185, 46, 49, 255) : new Color32(46, 185, 132, 255);
            
            DOTween.To(() => _disc.Radius, x => _disc.Radius = x, SELECTED_DISC_RADIUS, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Thickness, x => _disc.Thickness = x, SELECTED_DISC_THICKNESS, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);

            _thumbnail.rectTransform.DOScale(Vector3.one, .3f).SetUpdate(true);
            _energyButton.Rect.DOScale(Vector3.one * .75f, .3f).SetEase(Ease.OutExpo).SetUpdate(true);
        }

        public void Deselect()
        {
            if (!_isAvailable) return;

            deselected.Invoke(this);
            _isSelected = false;

            Color color = _isDisabled ? new Color32(185, 46, 49, DISABLED_ALPHA) : new Color32(46, 185, 132, DISABLED_ALPHA);
            
            DOTween.To(() => _disc.Radius, x => _disc.Radius = x, _initialDiscRadius, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Thickness, x => _disc.Thickness = x, _initialDiscThickness, .3f).SetUpdate(true);
            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);

            _thumbnail.rectTransform.DOScale(Vector3.one * 0.7f, .3f).SetUpdate(true);
            _energyButton.Rect.DOScale(Vector3.zero, .3f).SetUpdate(true);

            if (_isAvailable)
                _thumbnail.DOFade(1f, .5f).SetUpdate(true);
        }

        public void Disable() {
            _isDisabled = true;
            _energyButton.Disable();

            Color color = _isSelected
                ? new Color32(185, 46, 49, 255)
                : !_isAvailable ? new Color32(100, 100, 100, DISABLED_ALPHA)
                : new Color32(185, 46, 49, DISABLED_ALPHA);
            
            if (!_isAvailable)
                _thumbnail.DOFade(.5f, .5f).SetDelay(.5f).SetUpdate(true);

            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetDelay(.5f).SetUpdate(true);
        }

        public void Enable() {
            if (!_isAvailable) return;

            _isDisabled = false;
            _energyButton.Enable();

            Color color = _isSelected ? new Color32(46, 185, 132, 255) : new Color32(46, 185, 132, DISABLED_ALPHA);

            DOTween.To(() => _disc.Color, x => _disc.Color = x, color, .3f).SetUpdate(true);
            _thumbnail.DOFade(1f, .5f).SetUpdate(true);
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