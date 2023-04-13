using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using Game.Core;

namespace Game.UI
{
    public class UIRankRow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _position;

        [SerializeField]
        private TMP_InputField _input;

        [SerializeField]
        private TextMeshProUGUI _score;

        [SerializeField]
        private Color _activatedColor;

        [SerializeField]
        private Color _deactivatedColor;

        private LeaderboardPosition _data;
        private RectTransform _rect;

        public Action<UIRankRow> activated = delegate { };
        public Action<UIRankRow> deactivated = delegate { };
        public Action<UIRankRow> submitted = delegate { };

        public string Name => _input.text;
        public int Score => Int32.Parse(_score.text.Replace(".", ""));
        public RectTransform Rect => _rect;

        public void Awake()
        {
            _input.onEndEdit.AddListener(OnSubmit);
            TryGetComponent<RectTransform>(out _rect);
        }

        public void OnDestroy()
        {
            _input.onEndEdit.RemoveListener(OnSubmit);
        }

        private void OnSubmit(string value)
        {
            _data.Name = value;

            submitted.Invoke(this);
        }

        public void SetActive(bool value)
        {
            if (value) Activate();
            if (!value) Deactivate();
        }

        public void Activate()
        {
            _input.interactable = true;
            _input.textComponent.color = _activatedColor;
            _position.color = _activatedColor;
            _score.color = _activatedColor;

            _input.Select();

            activated.Invoke(this);
        }

        public void Deactivate()
        {
            _input.interactable = false;
            _input.textComponent.color = _deactivatedColor;
            _position.color = _deactivatedColor;
            _score.color = _deactivatedColor;

            deactivated.Invoke(this);
        }

        public void SetData(LeaderboardPosition data)
        {
            _input.text = $"{data.Name.Substring(0, Math.Min(3, data.Name.Length))}";
            _score.text = $"{data.Score:N0}".Replace(",", ".");
            _data = data;
        }
    }
}