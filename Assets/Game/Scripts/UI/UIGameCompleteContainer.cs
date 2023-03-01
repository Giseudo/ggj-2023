using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Combat;
using DG.Tweening;

namespace Game.UI
{
    public class UIGameCompleteContainer : MonoBehaviour
    {
        [SerializeField]
        private UIButton _menuButton;

        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            MatchManager.LevelCompleted += OnLevelComplete;
            _menuButton.clicked += MainMenu;
        }

        public void OnDestroy()
        {
            MatchManager.LevelCompleted -= OnLevelComplete;
            _menuButton.clicked -= MainMenu;
        }

        public void MainMenu()
        {
            GameManager.Scenes.LoadMenuScene();
        }

        private void OnLevelComplete()
        {
            if (GameManager.Scenes.CurrentLevel < GameManager.Scenes.LevelScenes.Count - 1) return;

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, .5f);
        }
    }
}