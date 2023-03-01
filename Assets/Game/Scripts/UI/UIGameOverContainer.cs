using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Combat;
using DG.Tweening;

namespace Game.UI
{
    public class UIGameOverContainer : MonoBehaviour
    {
        [SerializeField]
        private UIBlur _blur;

        [SerializeField]
        private UIButton _menuButton;

        [SerializeField]
        private UIButton _restartButton;

        private Damageable _damageable;
        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            GameManager.MainTree.TryGetComponent<Damageable>(out _damageable);
            _menuButton.clicked += MainMenu;
            _restartButton.clicked += RestartLevel;

            if (_damageable == null) return;

            _damageable.died += OnDie;
        }

        public void OnDestroy()
        {
            _menuButton.clicked -= MainMenu;
            _restartButton.clicked -= RestartLevel;

            if (_damageable == null) return;

            _damageable.died += OnDie;
        }

        public void MainMenu()
        {
            GameManager.Scenes.LoadMenuScene();
        }

        public void RestartLevel()
        {
            GameManager.Scenes.RestartLevel();

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, .5f);
            _blur.Hide();
        }

        private void OnDie(Damageable damageable)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, .5f);
            _blur.Show();
        }
    }
}