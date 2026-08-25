using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEditor.Search;

public class InventorySlot : MonoBehaviour
{
    RectTransform rect;
    Image image;
    InventoryManager inventoryManager;

    public Item item;
    private int slotIndex;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        inventoryManager = FindAnyObjectByType<InventoryManager>();
        UpdateSlot(null);
    }


    private Vector2 CalculateSlotPosition(int index)
    {
        int d = inventoryManager.SlotUIDistance;
        float statPos = -(((inventoryManager.SlotSize - 1) / 2.0f) * d);
        return new Vector2(statPos + (index * d), rect.anchoredPosition.y);
    }

    public void Initialize(int index)
    {
        slotIndex = index;
        rect.anchoredPosition = CalculateSlotPosition(slotIndex);
    }


    public void UpdateSlot(Item item)
    {
        Transform iconTransform = transform.Find("Item");
        Image icon = iconTransform.GetComponent<Image>();

        if (item)
        {
            this.item = item;
            icon.sprite = item.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            this.item = null;
            icon.sprite = null;
            
        }
    }

    public void Highlight()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(1.2f, 0.1f));
        seq.Append(rect.DOScale(1f, 0.1f));
    }
}
