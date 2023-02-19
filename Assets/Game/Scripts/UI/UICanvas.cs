using UnityEngine;
using Game.Core;

namespace Game.UI
{
    public class UICanvas : MonoBehaviour
    {
        public static UICanvas Instance;
        public static RectTransform Rect { get; private set; }
        public static Canvas MainCanvas { get; private set; }

        public void Awake()
        {
            Instance = this;
            Rect = GetComponent<RectTransform>();
            MainCanvas = GetComponent<Canvas>();
        }

        public static Vector2 GetScreenPosition(Vector3 worldPosition)
        {
            Camera camera = GameManager.MainCamera;
            Vector2 adjustedPosition = camera.WorldToScreenPoint(worldPosition);

            adjustedPosition.x *= Rect.rect.width / (float) camera.pixelWidth;
            adjustedPosition.y *= Rect.rect.height / (float) camera.pixelHeight;

            return adjustedPosition - Rect.sizeDelta / 2f;
        }
    }
}