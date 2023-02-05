using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Combat;
using DG.Tweening;

namespace Game.UI
{
    public class UILevelCompleteContainer : MonoBehaviour
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnLevelComplete()
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, .5f);
        }
    }
}