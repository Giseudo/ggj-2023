using System.Collections;
using UnityEngine;
using Game.Core;
using DG.Tweening;
using TMPro;

namespace Game.UI
{
    public class UICongratulationsContainer : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _titleText;

        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void Start()
        {
            GameManager.GameEnded += OnGameEnd;
        }

        public void OnDestroy()
        {
            GameManager.GameEnded -= OnGameEnd;
        }

        private void OnGameEnd()
        {
            StartCoroutine(DelayedShow(1f));
            StartCoroutine(ResetGame());
        }

        public void Show()
        {
            DOTween.To(() => _titleText.characterSpacing, x => _titleText.characterSpacing = x, 7f, 6f)
                .SetUpdate(true);

            _canvasGroup.DOFade(1f, 1f).SetUpdate(true);
        }

        public void Hide()
        {
            _canvasGroup.DOFade(0f, 1f)
                .SetUpdate(true)
                .OnComplete(() => _titleText.characterSpacing = 0f);
        }

        private IEnumerator DelayedShow(float waitTime)
        {
            yield return new WaitForSecondsRealtime(waitTime);

            Show();
        }

        private IEnumerator ResetGame()
        {
            yield return new WaitForSecondsRealtime(4f);

            GameManager.Scenes.LoadMenuScene();
        }
    }
}