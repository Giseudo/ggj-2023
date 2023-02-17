using System;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Game.Core
{
    public enum TimeState
    {
        Playing,
        Paused,
        FastForwarding,
    }

    public class TimeManager : MonoBehaviour
    {
        private float _currentScale;
        public static float CurrentScale { get; private set; }

        public static TimeManager Instance { get; private set; }
        public static TimeState State { get; private set; }
        public static Action<TimeState> StateChanged;

        public void Awake()
        {
            Instance = this;
            CurrentScale = 1f;
            State = TimeState.Playing;
            StateChanged = delegate { };
        }

        public static void Pause()
        {
            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, 0f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            SetState(TimeState.Paused);
        }

        public static void SlowMotion()
        {
            if (State == TimeState.Paused)
                return;

            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, .25f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);
        }

        public static void Play()
        {
            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, 1f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            SetState(TimeState.Playing);
        }

        public static void Resume()
        {
            if (State == TimeState.Playing) Play();
            if (State == TimeState.FastForwarding) FastForward();
        }

        public static void FastForward()
        {
            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, 2f, 1f)
                .SetEase(Ease.OutExpo)
                .SetUpdate(true);

            SetState(TimeState.FastForwarding);
        }

        private static void SetState(TimeState state)
        {
            State = state;
            StateChanged.Invoke(State);
        }
    }
}