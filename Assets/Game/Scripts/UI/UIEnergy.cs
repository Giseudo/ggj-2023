using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using TMPro;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
    using Game.Combat;

    public class UIEnergy : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _text;

        [SerializeField]
        private Image _image;

        [SerializeField]
        private Sprite _neutralSprite;

        [SerializeField]
        private Sprite _happySprite;

        [SerializeField]
        private VisualEffect _sparksVfx;

        [SerializeField]
        private RectTransform _wrapperRect;

        [SerializeField]
        private RectTransform _mainCanvasRect;

        private Tree _tree;
        private Tween _tween;
        private RectTransform _rect;
        private VFXEventAttribute _eventAttribute;

        public void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _eventAttribute = _sparksVfx.CreateVFXEventAttribute();
        }

        public void Start()
        {
            _tree = GameManager.MainTree;

            if (_tree == null) return;

            _text.text = $"{_tree.EnergyAmount}";

            _tree.collectedEnergy += OnEnergyChange;
            // _tree.consumedEnergy += OnEnergyChange;
        }

        public void OnDestroy()
        {
            if (_tree == null) return;

            _tree.collectedEnergy -= OnEnergyChange;
            // _tree.consumedEnergy -= OnEnergyChange;
        }

        private void OnEnergyChange(int amount, Vector3 position)
        {
            Vector3 targetPosition = GetScreenPosition(position);

            _sparksVfx.SetVector3("TargetPosition_position", position);
            _eventAttribute.SetVector3("TargetPosition_position", position);
            _sparksVfx.SendEvent("OnSpark", _eventAttribute);

            _wrapperRect.DOScale(Vector3.one * 1.2f, .2f)
                .SetDelay(1.5f)
                .OnStart(() => {
                    _image.sprite = _happySprite;
                    _text.text = $"{_tree.EnergyAmount}";
                })
                .OnComplete(() => {
                    _wrapperRect.DOScale(Vector3.one, .2f)
                        .SetDelay(.5f)
                        .OnComplete(() => _image.sprite = _neutralSprite);
                });

        }

        private Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            Camera camera = GameManager.MainCamera;
            Vector2 adjustedPosition = camera.WorldToScreenPoint(worldPosition);

            adjustedPosition.x *= _mainCanvasRect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= _mainCanvasRect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - _mainCanvasRect.sizeDelta / 2f;
        }
    }
}