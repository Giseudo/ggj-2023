using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Shapes;
using DG.Tweening;
using Game.Combat;
using Game.Core;

namespace Game.UI
{
    public class UIWaveTimer : MonoBehaviour, IPointerClickHandler
    {
        public const float START_ANGLE = 90f;
        public const float END_ANGLE = -270f;

        [SerializeField]
        private Disc _progressDisc;

        [SerializeField]
        private float _waitTime = 5f;

        [SerializeField]
        public int _earnEnergyAmount = 2000;

        private UIButton _button;
        private RectTransform _rect;
        private Sequence _sequence;
        private float _countdownTime;
        private bool _isDisabled = true;

        public Action<float> countdownStarted = delegate { };
        public Action<float> timeOver = delegate { };
        public Action<int> collected = delegate { };

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);
            TryGetComponent<UIButton>(out _button);

            _button.Disable();
        }

        public void Start()
        {
            MatchManager.RoundStarted += OnRoundStart;
        }

        public void OnDestroy()
        {
            MatchManager.RoundStarted -= OnRoundStart;
        }

        private void OnRoundStart(int roundNumber)
        {
            float longestTime = 20f;
            int lastRound = 0;

            MatchManager.WaveSpawners.ForEach(spawner => {
                Round round = spawner.Rounds[roundNumber];
                lastRound = spawner.Rounds.Count - 1;

                if (round.RoundTime > longestTime)
                    longestTime = round.RoundTime;
            });

            if (roundNumber >= lastRound)
                return;

            StartCoroutine(StartCountdown(longestTime));
        }

        private IEnumerator StartCountdown(float delay)
        {
            yield return new WaitForSeconds(delay);

            countdownStarted.Invoke(Time.time);
            _countdownTime = Time.time;
            _button.Enable();
            _isDisabled = false;

            _sequence = DOTween.Sequence();
            _sequence.Append(_rect.DOScale(Vector3.one * 1.2f, .5f).SetEase(Ease.OutSine));
            _sequence.Append(_rect.DOScale(Vector3.one, .5f).SetEase(Ease.InSine));
            _sequence.Append(
                DOTween.To(
                    () => START_ANGLE,
                    x => _progressDisc.AngRadiansStart = x * Mathf.Deg2Rad,
                    END_ANGLE,
                    _waitTime
                )
                    .SetEase(Ease.Linear)
                    .OnComplete(() => timeOver.Invoke(Time.time) )

            );
            _sequence.Append(_rect.DOScale(Vector3.one * 1.2f, .3f)
                .SetUpdate(true)
                .SetEase(Ease.OutSine)
            );
            _sequence.Append(
                _rect.DOScale(Vector3.zero, .2f)
                .SetUpdate(true)
                .SetEase(Ease.InExpo)
                .OnComplete(() => _progressDisc.AngRadiansStart = START_ANGLE)
            );
            _sequence.OnComplete(MatchManager.NextRound);
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (_isDisabled) return;

            _button.Disable();
            _isDisabled = true;
            _sequence.Kill();

            collected.Invoke(_earnEnergyAmount);

            MatchManager.NextRound();

            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);
            Vector3 position = Vector3.zero;

            if (Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                position = groundHit.point;

            MatchManager.DropEnergy(_earnEnergyAmount, position);

            _rect.DOScale(Vector3.one * 1.5f, .3f)
                .SetUpdate(true)
                .SetEase(Ease.InSine)
                .OnComplete(() => _rect.DOScale(Vector3.zero, .2f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutExpo)
                        .OnComplete(() => _progressDisc.AngRadiansStart = START_ANGLE)
                );
        }
    }
}