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

        public bool IsOpened => _isOpened;
        public RectTransform Rect => _rect;

        public Action<UnitData> selectedUnit = delegate { };
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
            _rect.DOScale(Vector3.one, .5f)
                .SetEase(Ease.OutExpo);
            _isOpened = true;
            opened.Invoke();
        }

        public void Hide()
        {
            _rect.DOScale(Vector3.zero, .2f);
            _isOpened = false;
            closed.Invoke();
        }

        public void AddCard(UIUnitCard card)
        {
            _cards.Add(card);

            card.clicked += OnCardClick;
        }

        public void RemoveCard(UIUnitCard card)
        {
            _cards.Remove(card);

            card.clicked -= OnCardClick;
        }

        public void OnCardClick(UIUnitCard card)
        {
            selectedUnit.Invoke(card.Data);
        }
    }
}