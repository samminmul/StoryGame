using UnityEngine;

// 배경 이미지 범위 밖으로는 카메라가 나가지 않고, 캐릭터가 경계 쪽으로 이동하면
// 카메라는 멈춘 채 캐릭터만 화면 안에서 계속 움직이게 한다.
[RequireComponent(typeof(Camera))]
public class CameraFollowClamped2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;

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
        float halfHeight = cam.orthographicSize;

        float clampedMinX = minX + halfWidth;
        float clampedMaxX = maxX - halfWidth;
        float clampedMinY = minY + halfHeight;
        float clampedMaxY = maxY - halfHeight;

        float x = clampedMinX <= clampedMaxX
            ? Mathf.Clamp(target.position.x, clampedMinX, clampedMaxX)
            : (minX + maxX) * 0.5f;
        float y = clampedMinY <= clampedMaxY
            ? Mathf.Clamp(target.position.y, clampedMinY, clampedMaxY)
            : (minY + maxY) * 0.5f;

        Vector3 position = transform.position;
        position.x = x;
        position.y = y;
        transform.position = position;
    }
}
