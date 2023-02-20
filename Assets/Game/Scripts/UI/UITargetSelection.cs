using System;
using UnityEngine;
using Game.Core;
using Game.Combat;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class UITargetSelection : MonoBehaviour, IPointerMoveHandler, IPointerClickHandler
    {
        public const float RADIUS = 5f;

        [SerializeField]
        private UIRangeRadius _targetRangeRadius;

        [SerializeField]
        private UIRangeRadius _fovRadius;

        [SerializeField]
        private Color32 _invalidColor;

        private CanvasGroup _canvasGroup;

        public Action closed = delegate { };
        public Action<Vector3> confirmed = delegate { };
        private Unit _activeUnit;
        private bool _isValid;

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Show(Unit unit)
        {
            _activeUnit = unit;
            _targetRangeRadius.SetRadius(RADIUS);
            _fovRadius.SetRadius(unit.Data.RangeRadius);
            _fovRadius.transform.position = unit.transform.position;

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _targetRangeRadius.SetRadius(0f);
            _fovRadius.SetRadius(0f);

            closed.Invoke();
        }

        public void OnPointerMove(PointerEventData evt)
        {
            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;

            _targetRangeRadius.transform.position = groundHit.point + Vector3.up * .5f;

            float distance = (groundHit.point - _activeUnit.transform.position).magnitude;

            _isValid = distance + RADIUS < _activeUnit.Data.RangeRadius;

            if (_isValid)
                _targetRangeRadius.SetColor(_targetRangeRadius.InitialColor);
            else
                _targetRangeRadius.SetColor(_invalidColor);
        }

        public void OnPointerClick(PointerEventData evt)
        {
            if (!_isValid) return;
            if (evt.button != PointerEventData.InputButton.Left) return;

            Hide();

            confirmed.Invoke(_targetRangeRadius.transform.position);
        }
    }
}