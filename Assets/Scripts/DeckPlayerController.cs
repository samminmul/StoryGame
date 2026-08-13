using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class DeckPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float frameDuration = 0.15f;

    [SerializeField] private Sprite[] downFrames;
    [SerializeField] private Sprite[] upFrames;
    [SerializeField] private Sprite[] leftFrames;
    [SerializeField] private Sprite[] rightFrames;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Vector2 colliderWorldSize;
    private Vector2 colliderWorldOffset;
    private float frameTimer;
    private int frameIndex;
    private Sprite[] currentFrames;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            colliderWorldSize = boxCollider.bounds.size;
            colliderWorldOffset = (Vector2)boxCollider.bounds.center - (Vector2)transform.position;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        Vector2 direction = Vector2.zero;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            direction.x -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            direction.x += 1f;
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            direction.y += 1f;
        }
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            direction.y -= 1f;
        }

        Vector2 delta = direction.normalized * moveSpeed * Time.deltaTime;
        TryMove(new Vector3(delta.x, 0f, 0f));
        TryMove(new Vector3(0f, delta.y, 0f));

        UpdateAnimation(direction);
    }

    // 건물 등 트리거가 아닌 콜라이더를 향해 이동하려는 축만 막고, 나머지 축은 그대로 이동시켜
    // 벽을 따라 미끄러지듯 움직이게 한다.
    private void TryMove(Vector3 delta)
    {
        if (delta == Vector3.zero)
        {
            return;
        }

        Vector3 targetPosition = transform.position + delta;

        if (boxCollider != null && IsBlocked(targetPosition))
        {
            return;
        }

        transform.position = targetPosition;
    }

    private bool IsBlocked(Vector3 targetPosition)
    {
        Vector2 targetCenter = (Vector2)targetPosition + colliderWorldOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(targetCenter, colliderWorldSize, 0f);
        foreach (Collider2D hit in hits)
        {
            if (hit == boxCollider || hit.isTrigger)
            {
                continue;
            }
            return true;
        }
        return false;
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            frameTimer = 0f;
            frameIndex = 0;
            currentFrames = null;
            if (downFrames != null && downFrames.Length > 0)
            {
                spriteRenderer.sprite = downFrames[0];
            }
            return;
        }

        Sprite[] targetFrames;
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            targetFrames = direction.x > 0f ? rightFrames : leftFrames;
        }
        else
        {
            targetFrames = direction.y > 0f ? upFrames : downFrames;
        }

        if (targetFrames == null || targetFrames.Length == 0)
        {
            return;
        }

        if (targetFrames != currentFrames)
        {
            currentFrames = targetFrames;
            frameTimer = 0f;
            frameIndex = 0;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % currentFrames.Length;
        }

        spriteRenderer.sprite = currentFrames[frameIndex];
    }
}
