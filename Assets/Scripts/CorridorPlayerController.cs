using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer))]
public class CorridorPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float minX = -8.5f;
    [SerializeField] private float maxX = 8.5f;
    [SerializeField] private float upperFloorSplitY = 0f;
    [SerializeField] private string leftExitDestination = "OO";
    [SerializeField] private string upperExitScene = "갑판 앞";
    [SerializeField] private string upperLeftExitScene = "갑판 뒤";
    [SerializeField] private string lowerRightExitScene = "선장실";
    [SerializeField] private float frameDuration = 0.15f;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] leftFrames;
    [SerializeField] private Sprite[] rightFrames;

    private bool loggedLeftExit;
    private bool loggedRightExit;
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

        float direction = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            direction -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            direction += 1f;
        }

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x + direction * moveSpeed * Time.deltaTime, minX, maxX);
        transform.position = position;

        UpdateAnimation(direction);

        if (position.x <= minX)
        {
            if (!loggedLeftExit)
            {
                loggedLeftExit = true;
                if (position.y > upperFloorSplitY)
                {
                    // 2층(위층) 왼쪽 끝은 갑판 뒤 씬으로 전환한다.
                    SceneManager.LoadScene(upperLeftExitScene);
                }
                else
                {
                    Debug.Log($"복도 왼쪽 끝 도달: {leftExitDestination}로 이동");
                }
            }
        }
        else
        {
            loggedLeftExit = false;
        }

        if (position.x >= maxX)
        {
            if (!loggedRightExit)
            {
                loggedRightExit = true;
                if (position.y > upperFloorSplitY)
                {
                    // 2층(위층) 오른쪽 끝은 갑판 씬으로 전환한다.
                    SceneManager.LoadScene(upperExitScene);
                }
                else
                {
                    // 1층(아래층) 오른쪽 끝은 선장실 씬으로 전환한다.
                    SceneManager.LoadScene(lowerRightExitScene);
                }
            }
        }
        else
        {
            loggedRightExit = false;
        }
    }

    private void UpdateAnimation(float direction)
    {
        if (direction == 0f)
        {
            frameTimer = 0f;
            frameIndex = 0;
            currentFrames = null;
            spriteRenderer.flipX = false;
            if (idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
            return;
        }

        Sprite[] targetFrames = direction > 0f ? rightFrames : leftFrames;
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
