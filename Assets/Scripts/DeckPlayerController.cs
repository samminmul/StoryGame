using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class DeckPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float frameDuration = 0.15f;

    // frames[0,1]: 아래쪽, frames[2,3]: 위쪽, frames[4,5,6]: 옆(왼쪽 기준, 오른쪽은 좌우 반전)
    [SerializeField] private Sprite[] frames;

    private SpriteRenderer spriteRenderer;
    private float frameTimer;
    private int frameIndex;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        transform.position += (Vector3)(direction.normalized * moveSpeed * Time.deltaTime);

        UpdateAnimation(direction);
    }

    private void UpdateAnimation(Vector2 direction)
    {
        if (frames == null || frames.Length < 7)
        {
            return;
        }

        if (direction == Vector2.zero)
        {
            frameTimer = 0f;
            frameIndex = 0;
            return;
        }

        int start;
        int length;
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y))
        {
            start = 4;
            length = 3;
            spriteRenderer.flipX = direction.x > 0f;
        }
        else if (direction.y > 0f)
        {
            start = 2;
            length = 2;
            spriteRenderer.flipX = false;
        }
        else
        {
            start = 0;
            length = 2;
            spriteRenderer.flipX = false;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex = (frameIndex + 1) % length;
        }
        if (frameIndex >= length)
        {
            frameIndex = 0;
        }

        spriteRenderer.sprite = frames[start + frameIndex];
    }
}
