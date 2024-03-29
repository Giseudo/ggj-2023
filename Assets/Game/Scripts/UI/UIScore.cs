using System;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
    public class UIScore : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _valueText;

        [SerializeField]
        private VisualEffect _sparksVfx;

        [SerializeField]
        private AudioClip _collectClip;

        private RectTransform _rect;
        private VFXEventAttribute _eventAttribute;

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);
            _eventAttribute = _sparksVfx.CreateVFXEventAttribute();
        }

        public void Start()
        {
            MatchManager.ScoreChanged += OnScoreChange;

            SetValue(0);
        }

        public void OnDestroy()
        {
            MatchManager.ScoreChanged -= OnScoreChange;
        }

        public void SetValue(int value)
        {
            _valueText.text = $"{value:N0}".Replace(",", ".");
        }

        private Vector3 _originPosition;

        public void SetOriginPosition(Vector3 position)
        {
            _originPosition = position;
        }

        private void OnScoreChange(int value)
        {
            SetValue(value);

            SoundManager.PlaySound(_collectClip, .5f);

            _eventAttribute.SetVector3(Shader.PropertyToID("TargetPosition"), _originPosition + transform.up * -2f + transform.forward * 10f);
            _sparksVfx.SendEvent("OnSpark", _eventAttribute);
        }

        public void Show()
        {
            _rect.DOAnchorPosY(0f, 1f);
        }

        public void Hide()
        {
            _rect.DOAnchorPosY(80f, 1f);
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