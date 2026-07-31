using UnityEngine;

// 마우스 커서가 오브젝트 위에 올라가면 highlightSprite로, 벗어나면 원래 이미지로 되돌린다.
[RequireComponent(typeof(Collider2D))]
public class HoverHighlight : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite highlightSprite;

    private Sprite normalSprite;

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

    private void OnMouseEnter()
    {
        if (spriteRenderer != null && highlightSprite != null)
        {
            spriteRenderer.sprite = highlightSprite;
        }
    }

    private void OnMouseExit()
    {
        if (spriteRenderer != null && highlightSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }
}
