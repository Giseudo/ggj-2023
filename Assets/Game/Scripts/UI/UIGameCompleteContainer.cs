using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Combat;
using DG.Tweening;

namespace Game.UI
{
    public class UIGameCompleteContainer : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            MatchManager.LevelCompleted += OnLevelComplete;
        }

        public void OnDestroy()
        {
            MatchManager.LevelCompleted -= OnLevelComplete;
        }

        public void RestartGame()
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