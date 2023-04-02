using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace Game.UI
{
    public class UIRankRow : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI position;

        [SerializeField]
        private TMP_InputField input;

        [SerializeField]
        private TextMeshProUGUI score;

        [SerializeField]
        private Color activatedColor;

        [SerializeField]
        private Color deactivatedColor;

        public Action<UIRankRow> activated = delegate { };
        public Action<UIRankRow> deactivated = delegate { };

        public void OnEnable()
        {
            // Deactivate();
        }

        public void SetActive(bool value)
        {
            if (value) Activate();
            if (!value) Deactivate();
        }

        public void Activate()
        {
            input.interactable = true;
            input.textComponent.color = activatedColor;
            position.color = activatedColor;
            score.color = activatedColor;

            input.Select();

            activated.Invoke(this);
        }

        public void Deactivate()
        {
            input.interactable = false;
            input.textComponent.color = deactivatedColor;
            position.color = deactivatedColor;
            score.color = deactivatedColor;

            deactivated.Invoke(this);
        }
    }
}