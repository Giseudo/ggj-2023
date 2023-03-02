using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
    public class UICameraPan : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField]
        private float _speed = 7f;

        [SerializeField]
        private float _offsetLimit = 10f;

        private Vector3 _initialCameraPosition;
        private Tween _tween;
        private bool _isDisabled;
        public Action started = delegate { };
        public Action updated = delegate { };
        public Action finished = delegate { };

        public void Start()
        {
            _initialCameraPosition = GameManager.MainCamera.transform.position;
        }

        public void OnBeginDrag(PointerEventData evt)
        {
            if (_isDisabled) return;

            _tween?.Kill();

            started.Invoke();
        }

        public void OnDrag(PointerEventData evt)
        {
            if (_isDisabled) return;

            Camera camera = GameManager.MainCamera;
            float delta = evt.delta.x / UICanvas.MainCanvas.scaleFactor;

            Vector3 previousPosition = camera.transform.position;
            camera.transform.position -= camera.transform.right * delta * Time.unscaledDeltaTime * _speed;

            float displacement = (camera.transform.position - _initialCameraPosition).magnitude;

            if (displacement > _offsetLimit)
                camera.transform.position = previousPosition;
            
            updated.Invoke();
        }

        public void OnEndDrag(PointerEventData evt)
        {
            if (_isDisabled) return;

            Camera camera = GameManager.MainCamera;

            _tween = camera.transform.DOMove(_initialCameraPosition, .5f)
                .SetUpdate(true);

            finished.Invoke();
        }

        public void Enable()
        {
            _isDisabled = false;
        }

        public void Disable()
        {
            _isDisabled = true;
        }
    }
}