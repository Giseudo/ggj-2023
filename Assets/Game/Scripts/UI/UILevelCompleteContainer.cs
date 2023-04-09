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
        private CanvasGroup _buttonsCanvasGroup;

        [SerializeField]
        private TextMeshProUGUI _titleText;

        private CanvasGroup _canvasGroup;

        private void OnLevelLoad(int level) => Animate(false);
        private void OnVictory() => Animate(true);
        private void OnGameOver() => Animate(true);

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            GameManager.Scenes.loadedLevel += OnLevelLoad;
            MatchManager.ScoreFinished += OnScoreFinish;

            if (_state == LevelState.Defeat)
                MatchManager.GameOver += OnGameOver;

            if (_state == LevelState.Victory)
                MatchManager.LevelCompleted += OnVictory;

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
            MatchManager.ScoreFinished -= OnScoreFinish;

            if (_state == LevelState.Defeat)
                MatchManager.GameOver -= OnGameOver;

            if (_state == LevelState.Victory)
                MatchManager.LevelCompleted -= OnVictory;

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
            _bodyCanvasGroup.DOFade(0f, .5f).SetUpdate(true);
            _buttonsCanvasGroup.DOFade(0f, .5f).SetUpdate(true)
                .OnComplete(GameManager.Scenes.LoadNextLevel);

            _buttonsCanvasGroup.interactable = false;
            _buttonsCanvasGroup.blocksRaycasts = false;
        }

        private void ShowActions()
        {
            _buttonsCanvasGroup.DOFade(1f, .5f)
                .SetUpdate(true)
                .OnComplete(() => {
                    _buttonsCanvasGroup.interactable = true;
                    _buttonsCanvasGroup.blocksRaycasts = true;
                });
        }

        private void RestartLevel()
        {
            GameManager.Scenes.RestartLevel();

            _bodyCanvasGroup.DOFade(0f, .5f).SetUpdate(true);
            _buttonsCanvasGroup.DOFade(0f, .5f).SetUpdate(true);

            _buttonsCanvasGroup.interactable = false;
            _buttonsCanvasGroup.blocksRaycasts = false;
        }

        private void OnScoreFinish()
        {
            if (GameManager.Scenes.IsLastLevel)
            {
                ShowActions();
                return;
            }

            GameManager.MainLight.TryGetComponent<SceneTransition>(out SceneTransition transition);

            transition.StartTransition(ShowActions);
        }

        private void Animate(bool value)
        {
            _menuButton.gameObject.SetActive(true);

            if (_state == LevelState.Victory)
                _continueButton.Rect.anchoredPosition = new Vector2(160f, _continueButton.Rect.anchoredPosition.y);

            if (value)
            {
                if (GameManager.Scenes.IsLastLevel && _state == LevelState.Victory)
                {
                    _titleText.text = "Fim de jogo !!";
                    _menuButton.gameObject.SetActive(false);
                    _continueButton.Rect.anchoredPosition = new Vector2(0f, _continueButton.Rect.anchoredPosition.y);
                }

                _blur.Show();

                DOTween.To(() => _titleText.characterSpacing, x => _titleText.characterSpacing = x, 7f, 6f)
                    .SetUpdate(true);

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.DOFade(1f, .5f)
                    .SetUpdate(true)
                    .OnComplete(() => {
                        _bodyCanvasGroup.DOFade(1f, .5f).SetUpdate(true);

                        if (_state == LevelState.Victory) return;

                        ShowActions();
                    });

                return;
            }

            _blur.Hide(.5f);

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, .5f)
                .SetDelay(.5f)
                .SetUpdate(true)
                .OnComplete(() => _titleText.characterSpacing = 0f);
        }
    }
}