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
    private float frameTimer;
    private int frameIndex;
    private Sprite[] currentFrames;

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
