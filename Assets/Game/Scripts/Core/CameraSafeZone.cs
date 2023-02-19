using UnityEngine;

[RequireComponent(typeof(Camera))]
[ExecuteInEditMode]
public class CameraSafeZone : MonoBehaviour
{
    private Camera _camera;

    public void OnEnable()
    {
        TryGetComponent<Camera>(out _camera);
    }

    public void OnDrawGizmos()
    {
        float height = (_camera.orthographicSize + 5f) * 2f;
        float width = ((_camera.orthographicSize + 12f) * 2f) * _camera.aspect;

        Gizmos.matrix = _camera.cameraToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(- Vector3.forward * _camera.farClipPlane / 2, new Vector3(width, height, _camera.farClipPlane));
    }
}
