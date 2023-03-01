using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Combat;
using DG.Tweening;
using TMPro;

namespace Game.UI
{
    public class UILevelCompleteContainer : MonoBehaviour
    {
        public enum LevelState
        {
            Victory,
            Defeat,
            Finished,
        }

        [SerializeField]
        private LevelState _state;

        [SerializeField]
        private UIBlur _blur;

        [SerializeField]
        private UIButton _menuButton;

        [SerializeField]
        private UIButton _continueButton;

        [SerializeField]
        private UIButton _restartButton;

        [SerializeField]
        private CanvasGroup _bodyCanvasGroup;

        [SerializeField]
        private TextMeshProUGUI _titleText;

        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;

            if (_state == LevelState.Defeat)
                MatchManager.GameOver += OnGameOver;

            if (_state == LevelState.Victory)
                MatchManager.LevelCompleted += OnVictory;
            
            if (_state == LevelState.Finished)
                MatchManager.GameCompleted += OnGameComplete;

            if (_menuButton)
                _menuButton.clicked += MainMenu;
            
            if (_continueButton)
                _continueButton.clicked += NextLevel;

            if (_restartButton)
                _restartButton.clicked += RestartLevel;
        }

        public void OnDestroy()
        {
            GameManager.Scenes.loadedLevel -= OnLevelLoad;

            if (_state == LevelState.Defeat)
                MatchManager.GameOver -= OnGameOver;

            if (_state == LevelState.Victory)
                MatchManager.LevelCompleted -= OnVictory;
            
            if (_state == LevelState.Finished)
                MatchManager.GameCompleted -= OnGameComplete;

            if (_menuButton)
                _menuButton.clicked -= MainMenu;
            
            if (_continueButton)
                _continueButton.clicked -= NextLevel;

            if (_restartButton)
                _restartButton.clicked -= RestartLevel;
        }

        public void MainMenu()
        {
            GameManager.Scenes.LoadMenuScene();
        }

        public void NextLevel()
        {
            if (!GameManager.MainLight.TryGetComponent<LightTransition>(out LightTransition transition)) return;

            transition.StartTransition(GameManager.Scenes.LoadNextLevel);
            _bodyCanvasGroup.DOFade(0f, .5f).SetUpdate(true);
        }

        private void RestartLevel()
        {
            GameManager.Scenes.RestartLevel();
            _bodyCanvasGroup.DOFade(0f, .5f).SetUpdate(true);
        }

        private void OnLevelLoad(int level) => Animate(false);
        private void OnVictory() => Animate(true);
        private void OnGameOver() => Animate(true);
        private void OnGameComplete() => Animate(true);

        private void Animate(bool value)
        {
            if (value)
            {
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.DOFade(1f, .5f)
                    .SetUpdate(true)
                    .OnComplete(() => {
                        _blur.Show();
                        _bodyCanvasGroup.DOFade(1f, .5f).SetUpdate(true);
                    });

                return;
            }

            _blur.Hide();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, .5f).SetUpdate(true);
        }
    }
}