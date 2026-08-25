using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 입구에 플레이어가 닿으면 하이라이트 이미지로 바뀌고, 그 상태에서 스페이스바를 누르면 targetScene으로 전환한다.
[RequireComponent(typeof(Collider2D))]
public class SceneEntrance : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite highlightSprite;
    [SerializeField] private string targetScene = "SampleScene";

    private bool playerInRange;
    private Sprite normalSprite;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            normalSprite = spriteRenderer.sprite;
        }
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        playerInRange = true;
        if (spriteRenderer != null && highlightSprite != null)
        {
            spriteRenderer.sprite = highlightSprite;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        playerInRange = false;
        if (spriteRenderer != null && highlightSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }
}
