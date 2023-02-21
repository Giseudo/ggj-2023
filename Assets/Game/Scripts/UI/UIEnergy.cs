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
            MatchManager.DroppedEnergy += OnDropEnergy;

            _tree = GameManager.MainTree;

            if (_tree == null) return;

            _text.text = $"{_tree.EnergyAmount}";

            _tree.collectedEnergy += OnCollectEnergy;
            _tree.consumedEnergy += OnConsumeEnergy;
        }

        public void OnDestroy()
        {
            MatchManager.DroppedEnergy -= OnDropEnergy;

            if (_tree == null) return;

            _tree.collectedEnergy -= OnCollectEnergy;
            _tree.consumedEnergy -= OnConsumeEnergy;
        }

        private void OnDropEnergy(int amount, Vector3 position)
        {
            Vector3 targetPosition = GetScreenPosition(position);

            _sparksVfx.SetVector3("TargetPosition_position", position);
            _eventAttribute.SetVector3("TargetPosition_position", position);
            _sparksVfx.SendEvent("OnSpark", _eventAttribute);
        }

        private void OnCollectEnergy(int amount)
        {
            _wrapperRect.DOScale(Vector3.one * 1.2f, .2f)
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

        private void OnConsumeEnergy(int amount)
        {
            _text.text = $"{_tree.EnergyAmount}";
        }

        private Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            Camera camera = GameManager.MainCamera;
            Vector2 adjustedPosition = camera.WorldToScreenPoint(worldPosition);

            adjustedPosition.x *= UICanvas.Rect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= UICanvas.Rect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - UICanvas.Rect.sizeDelta / 2f;
        }
    }
}