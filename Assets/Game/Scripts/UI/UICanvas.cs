using System;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;
using DG.Tweening;

namespace Game.UI
{
    public class UICanvas : MonoBehaviour
    {
        public static UICanvas Instance;
        public static Canvas Canvas { get; private set; }
        public static RectTransform Rect { get; private set; }
        public static Canvas MainCanvas { get; private set; }
        public static Vector2 ScreenSize { get; private set; }
        public static Image OverlayImage { get; private set; }
        public static Action<Vector2> ScreenResized;

        [SerializeField]
        private Image _overlayImage;

        public void Awake()
        {
            Instance = this;
            Canvas = GetComponent<Canvas>();
            Rect = GetComponent<RectTransform>();
            MainCanvas = GetComponent<Canvas>();
            OverlayImage = _overlayImage;
            ScreenSize = new Vector2(Screen.width, Screen.height);
            ScreenResized = delegate { };
        }

        public void Start()
        {
            GameManager.MainCameraChanged += OnCameraChange;
            Canvas.worldCamera = GameManager.MainCamera;

            _overlayImage.color = Color.black;
            _overlayImage.DOFade(0f, 2f);
        }

        public void OnDestroy()
        {
            GameManager.MainCameraChanged -= OnCameraChange;
        }

        private void OnCameraChange(Camera camera)
        {
            Canvas.worldCamera = camera;
        }

        public static Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            Camera camera = GameManager.MainCamera;
            Vector2 adjustedPosition = camera.WorldToScreenPoint(worldPosition);

            adjustedPosition.x *= Rect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= Rect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - Rect.sizeDelta / 2f;
        }

        public void Update()
        {
            CheckScreenSize();
        }

        public void CheckScreenSize()
        {
            if (ScreenSize.x == Screen.width && ScreenSize.y == Screen.height)
                return;

            ScreenSize = new Vector2(Screen.width, Screen.height);

            ScreenResized.Invoke(ScreenSize);
        }
    }
}