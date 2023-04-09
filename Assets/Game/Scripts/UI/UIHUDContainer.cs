using System.Collections;
using UnityEngine;
using Game.Core;
using Game.Combat;
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

        [SerializeField]
        private UIScore _score;

        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Vector2 _initialHealthPosition;
        private Vector2 _initialEnergyPosition;

        private void OnHealthDrain() => _score.SetOriginPosition(_health.Rect.position);

        private void OnEnergyDrain() => _score.SetOriginPosition(_energy.Rect.position);

        void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;
            MatchManager.LevelCompleted += OnLevelComplete;
            MatchManager.GameOver += OnLevelComplete;
            MatchManager.GameCompleted += OnGameComplete;
            MatchManager.DrainScoreHealth += OnHealthDrain;
            MatchManager.DrainScoreEnergy += OnEnergyDrain;

            _initialHealthPosition = _health.Rect.anchoredPosition;
            _initialEnergyPosition = _energy.Rect.anchoredPosition;

            _score.SetOriginPosition(_health.Rect.position);
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLevelLoad;
            MatchManager.LevelCompleted -= OnLevelComplete;
            MatchManager.GameOver -= OnLevelComplete;
            MatchManager.GameCompleted -= OnGameComplete;
            MatchManager.DrainScoreHealth -= OnHealthDrain;
            MatchManager.DrainScoreEnergy -= OnEnergyDrain;
        }

        private void OnGameComplete()
        {
            _canvasGroup.DOFade(0f, 1f).SetUpdate(true);
            _score.Hide();
            _energy.Hide();
            _health.Hide();
        }

        private void OnLevelLoad(int level)
        {
            // TODO move when button is clicked, instead level load
            Vector3 localPosition = _health.Rect.localPosition;

            _health.Rect.anchorMin = new Vector2(0f, 1f);
            _health.Rect.anchorMax = new Vector2(0f, 1f);
            _health.Rect.localPosition = localPosition;
            _health.Rect.DOAnchorPos(_initialHealthPosition, 1.5f);

            localPosition = _energy.Rect.localPosition;

            _energy.Rect.anchorMin = new Vector2(1f, 1f);
            _energy.Rect.anchorMax = new Vector2(1f, 1f);
            _energy.Rect.localPosition = localPosition;
            _energy.Rect.DOAnchorPos(_initialEnergyPosition, 1.5f);

            _time.Show(1f);
            _score.Hide();
            _energy.SetNeutralSprite();
        }

        private void OnLevelComplete()
        {
            Vector3 localPosition = _health.Rect.localPosition;

            _health.Rect.anchorMin = new Vector2(0.5f, 1f);
            _health.Rect.anchorMax = new Vector2(0.5f, 1f);
            _health.Rect.localPosition = localPosition;
            _health.Rect.DOAnchorPos(new Vector2(-40f, -100f), 1.5f);

            localPosition = _energy.Rect.localPosition;

            _energy.Rect.anchorMin = new Vector2(0.5f, 1f);
            _energy.Rect.anchorMax = new Vector2(0.5f, 1f);
            _energy.Rect.localPosition = localPosition;
            _energy.Rect.DOAnchorPos(new Vector2(40f, -100f), 1.5f);

            if (MatchManager.IsTreeDead)
                _energy.SetSadSprite();
            else
                _energy.SetHappySprite();

            _time.Hide();
            _score.Show();
        }
    }
}