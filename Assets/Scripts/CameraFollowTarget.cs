using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollowTarget : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private SpriteRenderer background;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (background == null)
        {
            GameObject bg = GameObject.Find("Background");
            if (bg != null)
            {
                background = bg.GetComponent<SpriteRenderer>();
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 position = transform.position;
        position.x = target.position.x;
        position.y = target.position.y;

        // 배경이 있으면 카메라 시야가 배경 밖으로 나가지 않게 X/Y를 각각 클램프한다.
        // 캐릭터는 계속 배경 끝까지 이동할 수 있고, 카메라만 더 이상 따라가지 않게 된다.
        if (background != null)
        {
            Bounds bounds = background.bounds;
            float halfWidth = cam.orthographicSize * cam.aspect;
            float halfHeight = cam.orthographicSize;

            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;
            position.x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : bounds.center.x;

            float minY = bounds.min.y + halfHeight;
            float maxY = bounds.max.y - halfHeight;
            position.y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : bounds.center.y;
        }

        transform.position = position;
    }
}
