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
        public Action<Vector3> updated = delegate { };
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
            Vector3 delta = camera.transform.right * (evt.delta.x / UICanvas.MainCanvas.scaleFactor) * Time.unscaledDeltaTime * _speed;

            Vector3 displacement = (camera.transform.position - delta) - _initialCameraPosition;
            Vector3 position = _initialCameraPosition + Vector3.ClampMagnitude(displacement, _offsetLimit);

            camera.transform.position = position;

            updated.Invoke(displacement);
        }

        public void OnEndDrag(PointerEventData evt)
        {
            if (_isDisabled) return;

            Camera camera = GameManager.MainCamera;

            _tween = camera.transform.DOMove(_initialCameraPosition, .5f)
                .OnUpdate(() => updated.Invoke(camera.transform.position - _initialCameraPosition))
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