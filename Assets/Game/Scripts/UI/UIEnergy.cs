using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using TMPro;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
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
        private AudioClip _collectClip;

        private Tween _tween;
        private RectTransform _rect;
        private VFXEventAttribute _eventAttribute;

        public RectTransform Rect => _rect;

        private void UpdateEnergy() => _text.text = $"{GameManager.MainTree.EnergyAmount}";
        private void OnConsumeEnergy(int amount) => UpdateEnergy();

        public void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _eventAttribute = _sparksVfx.CreateVFXEventAttribute();
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;
            MatchManager.DroppedEnergy += OnDropEnergy;

            GameManager.MainTree.collectedEnergy += OnCollectEnergy;
            GameManager.MainTree.consumedEnergy += OnConsumeEnergy;

            UpdateEnergy();
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLevelLoad;
            MatchManager.DroppedEnergy -= OnDropEnergy;
        }

        private void OnLevelLoad(int level)
        {
            GameManager.MainTree.collectedEnergy += OnCollectEnergy;
            GameManager.MainTree.consumedEnergy += OnConsumeEnergy;

            UpdateEnergy();
        }

        private float _lastPlaySoundTime;

        private void OnDropEnergy(int amount, Vector3 position)
        {
            Vector3 targetPosition = GetScreenPosition(position);

            if (_lastPlaySoundTime + .5f < Time.unscaledTime)
            {
                SoundManager.PlaySound(_collectClip, .5f);
                _lastPlaySoundTime = Time.unscaledTime;
            }

            _eventAttribute.SetVector3(Shader.PropertyToID("TargetPosition"), position);
            _sparksVfx.SendEvent("OnSpark", _eventAttribute);
        }

        private void OnCollectEnergy(int amount)
        {
            _wrapperRect.DOScale(Vector3.one * 1.2f, .2f)
                .OnStart(() => {
                    _image.sprite = _happySprite;
                    UpdateEnergy();
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

            adjustedPosition.x *= UICanvas.Rect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= UICanvas.Rect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - UICanvas.Rect.sizeDelta / 2f;
        }
    }
}