using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [SerializeField] private string interactionLabel = "문";
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite highlightSprite;

    private bool playerInRange;
    private Sprite normalSprite;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
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
            Debug.Log($"{interactionLabel}와(과) 상호작용함");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }
        playerInRange = true;
        if (promptRoot != null)
        {
            promptRoot.SetActive(true);
        }
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
        if (promptRoot != null)
        {
            promptRoot.SetActive(false);
        }
        if (spriteRenderer != null && highlightSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }
}
