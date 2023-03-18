using UnityEngine;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
    public class UIHUDContainer : MonoBehaviour
    {
        [SerializeField]
        private UIHealth _health;

        [SerializeField]
        private UITime _time;

        [SerializeField]
        private UIEnergy _energy;

        private RectTransform _rect;
        private Vector2 _initialHealthPosition;
        private Vector2 _initialEnergyPosition;

        void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);
        }

        void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;
            MatchManager.LevelCompleted += OnLevelComplete;
            MatchManager.GameOver += OnLevelComplete;

            _initialHealthPosition = _health.Rect.anchoredPosition;
            _initialEnergyPosition = _energy.Rect.anchoredPosition;
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLevelLoad;
            MatchManager.LevelCompleted -= OnLevelComplete;
            MatchManager.GameOver -= OnLevelComplete;
        }

        private void OnLevelLoad(int level)
        {
            Vector3 localPosition = _health.Rect.localPosition;

            _health.Rect.anchorMin = new Vector2(0f, 1f);
            _health.Rect.anchorMax = new Vector2(0f, 1f);
            _health.Rect.localPosition = localPosition;
            _health.Rect.DOAnchorPos(_initialHealthPosition, 2f);

            localPosition = _energy.Rect.localPosition;

            _energy.Rect.anchorMin = new Vector2(1f, 1f);
            _energy.Rect.anchorMax = new Vector2(1f, 1f);
            _energy.Rect.localPosition = localPosition;
            _energy.Rect.DOAnchorPos(_initialEnergyPosition, 2f);

            _time.Show(1f);
        }

        private void OnLevelComplete()
        {
            Vector3 localPosition = _health.Rect.localPosition;

            _health.Rect.anchorMin = new Vector2(0.5f, 1f);
            _health.Rect.anchorMax = new Vector2(0.5f, 1f);
            _health.Rect.localPosition = localPosition;
            _health.Rect.DOAnchorPosX(-40f, 2f);

            localPosition = _energy.Rect.localPosition;

            _energy.Rect.anchorMin = new Vector2(0.5f, 1f);
            _energy.Rect.anchorMax = new Vector2(0.5f, 1f);
            _energy.Rect.localPosition = localPosition;
            _energy.Rect.DOAnchorPosX(40f, 2f);

            _time.Hide();
        }
    }
}