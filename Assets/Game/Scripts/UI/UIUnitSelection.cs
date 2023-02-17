using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Game.Combat;

namespace Game.UI
{
    public class UIUnitSelection : MonoBehaviour
    {
        private RectTransform _rect;
        private List<UIUnitCard> _cards = new List<UIUnitCard>();
        private bool _isOpened = false;
        private Tween _tween;

        public bool IsOpened => _isOpened;
        public RectTransform Rect => _rect;

        public Action<UnitData> clicked = delegate { };
        public Action<UnitData> selected = delegate { };
        public Action opened = delegate { };
        public Action closed = delegate { };

        public void Awake()
        {
            TryGetComponent<RectTransform>(out _rect);

            UIUnitCard[] cards = GetComponentsInChildren<UIUnitCard>();

            for (int i = 0; i < cards.Length; i++)
            {
                UIUnitCard card = cards[i];

                AddCard(card);
            }
        }

        public void Show()
        {
            _tween?.Kill();
            _tween = _rect.DOScale(Vector3.one, .5f)
                .SetUpdate(true)
                .SetEase(Ease.OutExpo);

            _isOpened = true;
            opened.Invoke();
        }

        public void Hide()
        {
            _tween?.Kill();
            _tween = _rect.DOScale(Vector3.zero, .2f)
                .SetUpdate(true);

            _isOpened = false;
            closed.Invoke();
        }

        public void AddCard(UIUnitCard card)
        {
            _cards.Add(card);

            card.clicked += OnCardClick;
            card.selected += OnCardSelect;
            card.deselected += OnCardDeselect;
        }

        public void RemoveCard(UIUnitCard card)
        {
            _cards.Remove(card);

            card.clicked -= OnCardClick;
            card.selected -= OnCardSelect;
            card.deselected -= OnCardDeselect;
        }

        public void OnCardClick(UIUnitCard card)
        {
            clicked.Invoke(card.Data);
        }

        private void OnCardDeselect(UIUnitCard card)
        {
            selected.Invoke(null);
        }

        public void OnCardSelect(UIUnitCard card)
        {
            selected.Invoke(card.Data);

            for (int i = 0; i < _cards.Count; i++)
            {
                UIUnitCard currentCard = _cards[i];

                if (card == currentCard)
                    continue;
            }
        }
    }
}