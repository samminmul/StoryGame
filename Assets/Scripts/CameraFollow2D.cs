using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float minX = -8.5f;
    [SerializeField] private float maxX = 8.5f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        float halfWidth = cam.orthographicSize * cam.aspect;
        float clampedMinX = minX + halfWidth;
        float clampedMaxX = maxX - halfWidth;

        float x = clampedMinX <= clampedMaxX
            ? Mathf.Clamp(target.position.x, clampedMinX, clampedMaxX)
            : (minX + maxX) * 0.5f;

        Vector3 position = transform.position;
        position.x = x;
        transform.position = position;
    }
}
