using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
    public class UICreditsContainer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private Canvas _mainCanvas;

        [SerializeField]
        private List<Transform> _members;

        private Camera _mainCamera;
        private Transform _activeInteractable;
        private Transform _previousInteractable;
        private float _acceleration;
        private Tween _previousTween;
        private CanvasGroup _canvasGroup;

        public void Awake()
        {
            _mainCamera = Camera.main;
            TryGetComponent<CanvasGroup>(out _canvasGroup);
        }

        public void OnBeginDrag(PointerEventData evt)
        {
            Ray ray = _mainCamera.ScreenPointToRay(evt.position);

            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, 1 << LayerMask.NameToLayer("Interactable")))
                return;
            
            _acceleration = 0f;
            _activeInteractable = hit.collider.transform;

            if (_previousInteractable == _activeInteractable)
                _previousTween?.Kill();
        }

        public void OnDrag(PointerEventData evt)
        {
            if (_activeInteractable == null) return;

            _acceleration += evt.delta.x * Time.deltaTime;
            _activeInteractable.eulerAngles -= new Vector3(0f, evt.delta.x / _mainCanvas.scaleFactor, 0f);
        }

        public void OnEndDrag(PointerEventData evt)
        {
            if (_activeInteractable)
            {
                Transform target = _activeInteractable;
                float acceleration = _acceleration;

                _previousTween = DOTween.To(() => acceleration, x => target.eulerAngles -= new Vector3(0f, x, 0f), 0f, 1.5f);
            }
            
            _previousInteractable = _activeInteractable;
            _activeInteractable = null;
        }

        public void Show()
        {
            for (int i = 0; i < _members.Count; i++)
            {
                Transform member = _members[i];
                member.DOScale(Vector3.one, 1f).SetEase(Ease.OutElastic).SetDelay(i * 0.05f);
            }

            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOFade(1f, .5f)
                .SetUpdate(true);
        }

        public void Hide()
        {
            _members.ForEach(member => member.DOScale(Vector3.zero, .5f).SetEase(Ease.OutSine));

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.DOFade(0f, .5f)
                .SetUpdate(true);
        }

        public void Update()
        {
            if (_activeInteractable == null) return;

            if (Mathf.Abs(_acceleration) < .1f)
            {
                _acceleration = 0f;
                return;
            }

            _acceleration -= Mathf.Sign(_acceleration) * Time.deltaTime * 15f;
        }
    }
}