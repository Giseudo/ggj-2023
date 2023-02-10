using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Combat;
using Game.Core;
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

        public void Awake()
        {
            _pauseButton.onClick.AddListener(OnPause);
            _playButton.onClick.AddListener(OnPlay);
            _fastForwardButton.onClick.AddListener(OnFastForward);
        }

        public void OnDestroy()
        {
            _pauseButton.onClick.RemoveListener(OnPause);
            _playButton.onClick.RemoveListener(OnPlay);
            _fastForwardButton.onClick.RemoveListener(OnFastForward);
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
    }
}