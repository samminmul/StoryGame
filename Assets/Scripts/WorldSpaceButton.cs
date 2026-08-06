using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class WorldSpaceButton : MonoBehaviour
{
    [SerializeField] private Sprite highlightSprite;
    [SerializeField] private UnityEvent onClick;

    private SpriteRenderer spriteRenderer;
    private Sprite normalSprite;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        normalSprite = spriteRenderer.sprite;
    }

    private void OnMouseEnter()
    {
        if (highlightSprite != null)
        {
            spriteRenderer.sprite = highlightSprite;
        }
    }

    private void OnMouseExit()
    {
        spriteRenderer.sprite = normalSprite;
    }

    private void OnMouseDown()
    {
        onClick.Invoke();
    }
}
