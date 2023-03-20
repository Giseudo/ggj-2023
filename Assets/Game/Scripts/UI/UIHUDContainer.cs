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
            // TODO move when button is clicked, instead level load
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
            _score.Hide();
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
            _score.Show();

            StartCoroutine(AbsorbHealth());
        }

        private IEnumerator AbsorbHealth()
        {
            if (!GameManager.MainTree.TryGetComponent<Damageable>(out Damageable damageable)) yield return null;

            while (damageable.Health > 0)
            {
                _score.SetOriginPosition(_health.Rect.position);

                damageable.SetHealth(damageable.Health - 1);

                MatchManager.AddScore(10000);

                yield return new WaitForSeconds(.25f);
            }

            StartCoroutine(AbsorbEnergy());
        }

        private IEnumerator AbsorbEnergy()
        {
            while (GameManager.MainTree.EnergyAmount > 0)
            {
                _score.SetOriginPosition(_energy.Rect.position);

                int score = 10000;

                MatchManager.AddScore(score * 10);

                GameManager.MainTree.SetEnergy(GameManager.MainTree.EnergyAmount - score);

                yield return new WaitForSeconds(.25f);
            }
        }
    }
}