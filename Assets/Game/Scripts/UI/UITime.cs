using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using Game.Input;
using DG.Tweening;

namespace Game.UI
{
    public class UITime : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _selectionRect;

        [SerializeField]
        private UIButton _pauseButton;

        [SerializeField]
        private UIButton _playButton;

        [SerializeField]
        private UIButton _fastForwardButton;

        [SerializeField]
        private InputReader _inputReader;

        private RectTransform _rect;
        private TimeState _previousState;
        private TimeState _currentState;
        private Tween _resizeTween;
        private bool _isOpened = true;

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);

            _pauseButton.clicked += OnPause;
            _playButton.clicked += OnPlay;
            _fastForwardButton.clicked += OnFastForward;

            _inputReader.paused += OnPause;
            _inputReader.played += OnPlay;
            _inputReader.fastForwarded += OnFastForward;
            _inputReader.changedTime += OnChangeTime;
            _inputReader.toggledPlay += OnTogglePlay;
        }

        public void Start()
        {
            MatchManager.LevelCompleted += OnPlay;
            TimeManager.StateChanged += OnStateChange;
            UICanvas.ScreenResized += OnScreenResize;
        }

        public void OnDestroy()
        {
            _pauseButton.clicked -= OnPause;
            _playButton.clicked -= OnPlay;
            _fastForwardButton.clicked -= OnFastForward;

            _inputReader.paused -= OnPause;
            _inputReader.played -= OnPlay;
            _inputReader.fastForwarded -= OnFastForward;
            _inputReader.changedTime -= OnChangeTime;
            _inputReader.toggledPlay -= OnTogglePlay;

            MatchManager.LevelCompleted -= OnPlay;
            TimeManager.StateChanged -= OnStateChange;
            UICanvas.ScreenResized -= OnScreenResize;
        }

        public void OnPause() {
            if (!_isOpened) return;

            TimeManager.Pause();

            _selectionRect.DOAnchorPosY(_pauseButton.Rect.anchoredPosition.y, .3f)
                .SetUpdate(true);
        }
        public void OnPlay() {
            TimeManager.Play();

            _selectionRect.DOAnchorPosY(_playButton.Rect.anchoredPosition.y, .3f)
                .SetUpdate(true);
        }
        public void OnFastForward() {
            if (!_isOpened) return;

            TimeManager.FastForward();

            _selectionRect.DOAnchorPosY(_fastForwardButton.Rect.anchoredPosition.y, .3f)
                .SetUpdate(true);
        }

        private void OnChangeTime(float value)
        {
            if (!_isOpened) return;

            if (TimeManager.State == TimeState.Playing && value == -1)
                OnPause();

            else if (TimeManager.State == TimeState.FastForwarding && value == -1)
                OnPlay();

            else if (TimeManager.State == TimeState.Paused && value == 1)
                OnPlay();

            else if (TimeManager.State == TimeState.Playing && value == 1)
                OnFastForward();
        }

        private void OnTogglePlay()
        {
            if (!_isOpened) return;

            if (TimeManager.State == TimeState.Playing || TimeManager.State == TimeState.FastForwarding)
                OnPause();

            else if (_previousState == TimeState.FastForwarding)
                OnFastForward();

            else if (_previousState == TimeState.Playing)
                OnPlay();
        }

        private void OnStateChange(TimeState state)
        {
            _previousState = _currentState;
            _currentState = state;
        }

        private void OnScreenResize(Vector2 size)
        {
            UIButton button = null;

            if (TimeManager.State == TimeState.Paused)
                button = _pauseButton;

            if (TimeManager.State == TimeState.Playing)
                button = _playButton;

            if (TimeManager.State == TimeState.FastForwarding)
                button = _fastForwardButton;
        }

        public void Show(float delay = 0f)
        {
            if (_isOpened)
                return;

            _isOpened = true;
            _rect.DOAnchorPos(new Vector2(20f, _rect.anchoredPosition.y), 1f)
                .SetUpdate(true)
                .SetDelay(delay);
        }

        public void Hide(float delay = 0f)
        {
            if (!_isOpened)
                return;

            _isOpened = false;
            _rect.DOAnchorPos(new Vector2(-80f, _rect.anchoredPosition.y), 1f)
                .SetUpdate(true)
                .SetDelay(delay);
        }
    }
}