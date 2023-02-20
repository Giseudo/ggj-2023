using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;
using Game.Combat;
using DG.Tweening;

namespace Game.UI
{
    public class UIGameOverContainer : MonoBehaviour
    {
        private Damageable _damageable;
        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            GameManager.MainTree.TryGetComponent<Damageable>(out _damageable);

            if (_damageable == null) return;

            _damageable.died += OnDie;
        }

        public void OnDestroy()
        {
            if (_damageable == null) return;

            _damageable.died += OnDie;
        }

        public void RestartGame()
        {
            GameManager.Scenes.RestartLevel();
        }

        private void OnDie(Damageable damageable)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, .5f);
        }
    }
}