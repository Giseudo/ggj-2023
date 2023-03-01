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
        private Button _pauseButton;

        [SerializeField]
        private Button _playButton;

        [SerializeField]
        private Button _fastForwardButton;

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

            _pauseButton.onClick.AddListener(OnPause);
            _playButton.onClick.AddListener(OnPlay);
            _fastForwardButton.onClick.AddListener(OnFastForward);

            _inputReader.paused += OnPause;
            _inputReader.played += OnPlay;
            _inputReader.fastForwarded += OnFastForward;
            _inputReader.changedTime += OnChangeTime;
            _inputReader.toggledPlay += OnTogglePlay;
        }

        public void Start()
        {
            TimeManager.StateChanged += OnStateChange;
            UICanvas.ScreenResized += OnScreenResize;
        }

        public void OnDestroy()
        {
            _pauseButton.onClick.RemoveListener(OnPause);
            _playButton.onClick.RemoveListener(OnPlay);
            _fastForwardButton.onClick.RemoveListener(OnFastForward);

            _inputReader.paused -= OnPause;
            _inputReader.played -= OnPlay;
            _inputReader.fastForwarded -= OnFastForward;
            _inputReader.changedTime -= OnChangeTime;
            _inputReader.toggledPlay -= OnTogglePlay;

            TimeManager.StateChanged -= OnStateChange;
            UICanvas.ScreenResized -= OnScreenResize;
        }

        public void OnPause() {
            TimeManager.Pause();

            _selectionRect.DOMove(_pauseButton.transform.position, .3f)
                .SetUpdate(true);
        }
        public void OnPlay() {
            TimeManager.Play();

            _selectionRect.DOMove(_playButton.transform.position, .3f)
                .SetUpdate(true);
        }
        public void OnFastForward() {
            TimeManager.FastForward();

            _selectionRect.DOMove(_fastForwardButton.transform.position, .3f)
                .SetUpdate(true);
        }

        private void OnChangeTime(float value)
        {
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
            Button button = null;

            if (TimeManager.State == TimeState.Paused)
                button = _pauseButton;

            if (TimeManager.State == TimeState.Playing)
                button = _playButton;

            if (TimeManager.State == TimeState.FastForwarding)
                button = _fastForwardButton;

            StopCoroutine(FollowSelected(button));
            StartCoroutine(FollowSelected(button));
        }

        private IEnumerator FollowSelected(Button button)
        {
            yield return new WaitForSecondsRealtime(.2f);

            _resizeTween?.Kill();
            _resizeTween = _selectionRect.DOMove(button.transform.position, .3f)
                .SetUpdate(true);
        }

        public void Show()
        {
            if (_isOpened)
                return;

            _isOpened = true;
            _rect.DOAnchorPos(new Vector2(20f, _rect.anchoredPosition.y), 1f);
        }

        public void Hide()
        {
            if (!_isOpened)
                return;

            _isOpened = false;
            _rect.DOAnchorPos(new Vector2(-80f, _rect.anchoredPosition.y), 1f);
        }
    }
}