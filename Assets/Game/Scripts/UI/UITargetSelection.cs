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
        [SerializeField]
        private UIRangeRadius _targetRangeRadius;

        private CanvasGroup _canvasGroup;

        public Action<Vector3> confirmed = delegate { };

        public void Awake()
        {
            TryGetComponent<CanvasGroup>(out _canvasGroup);

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void Show(Unit unit)
        {
            _targetRangeRadius.SetRadius(5f);

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _targetRangeRadius.SetRadius(0f);
        }

        public void OnPointerMove(PointerEventData evt)
        {
            Ray ray = GameManager.MainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit groundHit, 100f, 1 << LayerMask.NameToLayer("Ground")))
                return;

            _targetRangeRadius.transform.position = groundHit.point + Vector3.up * .5f;
        }

        public void OnPointerClick(PointerEventData evt)
        {
            confirmed.Invoke(_targetRangeRadius.transform.position);
            Hide();
        }
    }
}