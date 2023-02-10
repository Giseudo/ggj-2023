using System;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Game.Core
{
    public class TimeManager : MonoBehaviour
    {
        private float _currentScale;
        public static float CurrentScale { get; private set; }

        public static TimeManager Instance { get; private set; }

        public void Awake()
        {
            Instance = this;
            CurrentScale = 1f;
        }

        public static void Pause()
        {
            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, 0f, .2f)
                .OnComplete(() => {
                    CurrentScale = 0f;
                    Time.timeScale = 0f;
                    Time.fixedDeltaTime = 0f;
                });
        }

        public static void Play()
        {
            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, 1f, .2f)
                .OnComplete(() => {
                    CurrentScale = 1f;
                    Time.timeScale = CurrentScale;
                    Time.fixedDeltaTime = CurrentScale * .02f;
                });
        }

        public static void FastForward()
        {
            DOTween.To(() => CurrentScale, x => {
                CurrentScale = x;

                Time.timeScale = x;
                Time.fixedDeltaTime = x * .02f;
            }, 2f, .2f)
                .OnComplete(() => {
                    CurrentScale = 2f;
                    Time.timeScale = CurrentScale;
                    Time.fixedDeltaTime = CurrentScale * .02f;
                });
        }
    }
}