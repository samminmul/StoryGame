using UnityEngine;
using UnityEngine.InputSystem;

public class ItemDebugger : MonoBehaviour
{
    InventoryManager inventoryManager;
    [SerializeField] private Item itemA;
    [SerializeField] private Item itemB;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryManager = FindAnyObjectByType<InventoryManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            inventoryManager.AddItem(itemA);
            Debug.Log($"Added item {itemA.ItemID} to inventory.");
        }
        else if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            inventoryManager.AddItem(itemB);
            Debug.Log($"Added item {itemB.ItemID} to inventory.");
        }
        else if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            inventoryManager.DeleteItem(itemA);
            Debug.Log($"Removed item {itemA.ItemID} from inventory.");
        }
        else if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            inventoryManager.DeleteItem(itemB);
            Debug.Log($"Removed item {itemB.ItemID} from inventory.");
        }
    }
}
