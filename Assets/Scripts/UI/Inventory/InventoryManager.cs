using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class InventoryManager : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    private List<GameObject> slots = new List<GameObject>();

    [SerializeField] private Transform inventoryPanel;
    [SerializeField] private Transform slotRow;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private int slotSize = 10;
    [SerializeField] private int slotUIDistance = 80;
    [SerializeField] private float inventoryMoveDist;

    private int slotPage = 0;
    private bool isInventoryOpen = false;
       

    public int SlotSize { get { return slotSize; } }
    public int SlotUIDistance { get { return slotUIDistance; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateSlots();
    }


    private void CreateSlots()
    {
        for (int i = 0; i < slotSize; i++)
        {
            GameObject slot = Instantiate(slotPrefab, slotRow);
            
            slot.GetComponent<InventorySlot>().Initialize(i);
            slots.Add(slot);
        }
    }

    private Item FindItemForSlot(int slotIndex)
    {
        int itemIndex = slotPage * slotSize + slotIndex;
        if (itemIndex < items.Count)
        {
            return items[itemIndex];
        }
        return null;
    }

    private void RefreshSlot(int slotIndex)
    {
        slots[slotIndex].GetComponent<InventorySlot>().UpdateSlot(FindItemForSlot(slotIndex));
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            RefreshSlot(i);
        }
    }

    private int AddItemToFirstEmptySlot(Item item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                return i;
            }
        }
        items.Add(item);
        return items.Count - 1;
    }

    private void setSlotPage(int newPage)
    {
        slotPage = newPage;
        RefreshAllSlots();
    }

    public void AddItem(Item item)
    {
        int itemIndex = AddItemToFirstEmptySlot(item);
        setSlotPage(itemIndex / slotSize);
        int slotIndexOfItem = itemIndex % slotSize;
        RefreshSlot(slotIndexOfItem);
        slots[slotIndexOfItem].GetComponent<InventorySlot>().Highlight();
    }

    public void DeleteItem(Item item)
    {
        items.Remove(item);
        if (slotPage > 0 && items.Count <= slotPage * slotSize)
        {
            PreviousPage();
        }
        RefreshAllSlots();
    }

    public bool HaveItem(string itemID)
    {
        foreach (Item item in items)
        {
            if (item != null && item.ItemID == itemID)
            {
                return true;
            }
        }
        return false;
    }

    public void NextPage()
    {
        if ((slotPage + 1) * slotSize < items.Count)
        {
            setSlotPage(slotPage + 1);
        }
    }

    public void PreviousPage()
    {
        if (slotPage > 0)
        {
            setSlotPage(slotPage - 1);
        }
    }

    private void OpenSlot()
    {
        inventoryPanel.gameObject.SetActive(true);
        inventoryPanel.GetComponent<RectTransform>().DOAnchorPosY(inventoryMoveDist, 0.5f).SetEase(Ease.OutBack);
        RefreshAllSlots();
    }

    private void CloseSlot()
    {
        inventoryPanel.GetComponent<RectTransform>().DOAnchorPosY(-inventoryMoveDist, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            inventoryPanel.gameObject.SetActive(false);
        });
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (isInventoryOpen)
        {
            OpenSlot();
        }
        else
        {
            CloseSlot();
        }
    }
}
