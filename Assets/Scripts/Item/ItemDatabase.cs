using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items;

    private Dictionary<string, ItemData> itemDictionary;

    public void Initialize()
    {
        itemDictionary = new Dictionary<string, ItemData>();

        foreach (ItemData item in items)
        {
            itemDictionary.Add(item.itemID, item);
        }
    }

    public ItemData GetItem(string itemID)
    {
        if (itemDictionary.TryGetValue(itemID, out ItemData item))
        {
            return item;
        }

        Debug.LogError($"Item ID를 찾을 수 없습니다: {itemID}");
        return null;
    }
}