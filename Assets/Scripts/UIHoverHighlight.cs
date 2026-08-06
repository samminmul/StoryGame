using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIHoverHighlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Sprite highlightSprite;

    private Image image;
    private Sprite normalSprite;

    private void Awake()
    {
        image = GetComponent<Image>();
        normalSprite = image.sprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (highlightSprite != null)
        {
            image.sprite = highlightSprite;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.sprite = normalSprite;
    }
}
