using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using DG.Tweening;
using TMPro;
using System;

namespace Game.UI
{
    public class UILeaderboardContainer : MonoBehaviour
    {
        [SerializeField]
        private List<UIRankRow> _rows = new List<UIRankRow>(5);

        [SerializeField]
        private TextMeshProUGUI _titleText;

        [SerializeField]
        private RectTransform _selector;

        [SerializeField]
        private UIButton _closeButton;

        [SerializeField]
        private LeaderboardData _data;

        private CanvasGroup _canvasGroup;
        private Tween _titleTween;
        private Tween _scaleTween;
        private Tween _rotationTween;

        public void OnSubmit(UIRankRow row) => DataHandler.SaveGameData();

        public void Awake()
        {
            _selector.localScale = Vector3.zero;
        }

        public void Start()
        {
            MatchManager.GameCompleted += OnGameComplete;
            MatchManager.NewHighScore += OnNewHighScore;

            _closeButton.clicked += OnClose;
            _rows.ForEach(row => {
                row.activated += OnRowActivate;
                row.submitted += OnSubmit;
            });

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_data == null)
                _data = MatchManager.Leaderboard;

            UpdateRows();
        }

        public void OnDestroy()
        {
            MatchManager.GameCompleted -= OnGameComplete;
            MatchManager.NewHighScore -= OnNewHighScore;

            _closeButton.clicked -= OnClose;
            _rows.ForEach(row => {
                row.activated -= OnRowActivate;
                row.submitted -= OnSubmit;
            });
        }

        public void Show()
        {
            DOTween.To(() => _titleText.characterSpacing, x => _titleText.characterSpacing = x, 7f, 3f)
                .SetUpdate(true);

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, .5f)
                .SetUpdate(true);
        }

        public void Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, .5f)
                .SetUpdate(true)
                .OnComplete(() => _titleText.characterSpacing = 0f);
            
            _titleTween?.Kill();
            _scaleTween?.Kill();
            _rotationTween?.Kill();
        }

        public void UpdateRows()
        {
            for (int i = 0; i < _rows.Count; i++)
            {
                UIRankRow row = _rows[i];
                LeaderboardPosition position = _data.GetPosition(i);
                
                row.SetData(position);
            }
        }

        private void OnNewHighScore(int position)
        {
            UpdateRows();

            Vector2 titlePosition = _titleText.rectTransform.anchoredPosition;

            _titleText.text = "Novo recorde !!";
            _titleText.rectTransform.anchoredPosition -= new Vector2(0, 10f);
            _titleTween = _titleText.rectTransform.DOAnchorPosY(titlePosition.y + 10f, 2f)
                .SetUpdate(true)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            if (position >= _rows.Count) return;

            UIRankRow row = _rows[position];

            StartCoroutine(ActivateRow(row));
        }

        private void OnRowActivate(UIRankRow row)
        {
            _selector.DOScale(Vector3.one, 1f)
                .SetUpdate(true)
                .OnComplete(() => _scaleTween = _selector.DOScale(Vector3.one * 1.25f, 1f)
                    .SetUpdate(true)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                );

            _selector.DOAnchorPosY(row.Rect.anchoredPosition.y, 1f)
                .SetUpdate(true);

            _rotationTween = _selector.DOLocalRotate(new Vector3(0, 0, 360f), 5f, RotateMode.FastBeyond360)
                .SetUpdate(true)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void OnClose()
        {
            Hide();

            if (GameManager.Instance == null) return;

            GameManager.GameOver();
        }

        public void OnGameComplete()
        {
            StartCoroutine(DelayedShow(1f));
        }

        private IEnumerator DelayedShow(float waitTime)
        {
            yield return new WaitForSecondsRealtime(waitTime);

            Show();
        }

        private IEnumerator ActivateRow(UIRankRow row)
        {
            yield return new WaitForSecondsRealtime(1f);

            row.Activate();
        }
    }
}