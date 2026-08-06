using UnityEngine;
using UnityEngine.InputSystem;

public class DraggableGauge : MonoBehaviour
{
    [SerializeField] private Transform handle;
    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float startValue = 1f;

    public float Value { get; private set; }

    // 트랙 전체가 아니라 핸들(아이콘) 자체를 클릭했을 때만 드래그가 시작되도록,
    // 핸들에 붙은 콜라이더로 히트 테스트한다.
    private Collider2D col;
    private bool dragging;

    private void Awake()
    {
        if (handle != null)
        {
            col = handle.GetComponent<Collider2D>();
        }
    }

    private void Start()
    {
        SetValue(startValue);
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Camera cam = Camera.main;
        if (mouse == null || cam == null || col == null)
        {
            return;
        }

        Vector2 screenPos = mouse.position.ReadValue();
        Vector3 worldPoint = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        worldPoint.z = 0f;

        if (mouse.leftButton.wasPressedThisFrame && col.OverlapPoint(worldPoint))
        {
            dragging = true;
        }
        else if (mouse.leftButton.wasReleasedThisFrame)
        {
            dragging = false;
        }

        if (dragging)
        {
            float clampedX = Mathf.Clamp(worldPoint.x, Mathf.Min(minX, maxX), Mathf.Max(minX, maxX));
            SetValue(Mathf.InverseLerp(minX, maxX, clampedX));
        }
    }

    private void SetValue(float value)
    {
        Value = value;
        if (handle != null)
        {
            Vector3 pos = handle.position;
            pos.x = Mathf.Lerp(minX, maxX, value);
            handle.position = pos;
        }
    }
}
