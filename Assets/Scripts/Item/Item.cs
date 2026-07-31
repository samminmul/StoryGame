using UnityEngine;
using DG.Tweening;

public class Item : MonoBehaviour
{
    Transform tr;
    private InventoryManager inventoryManager;

    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;
    [SerializeField] private string itemID;

    public string ItemID { get { return itemID; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tr = GetComponent<Transform>();
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }


    private void Disappear()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1.5f, 0.2f));
        seq.Append(transform.DOScale(0f, 0.2f));
        seq.OnComplete(() => gameObject.SetActive(false));
    }

    public void PickUp()
    {
        inventoryManager.AddItem(this);
        Disappear();
    }

    public void SetItemName(string name)
    {
        itemName = name;
    }

    public void SetItemDescription(string description)
    {
        itemDescription = description;
    }

    public string GetItemName()
    {
        return itemName;
    }

    public string GetItemDescription()
    {
        return itemDescription;
    }
}
