using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemSpawner : MonoBehaviour
{
    ItemDatabaseManager itemDatabaseManager;

    public static ItemSpawner instance;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            itemDatabaseManager = ItemDatabaseManager.Instance;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드 완료: {scene.name}");

        SpawnItemsForScene(scene.name);
    }

    public void SpawnItemsForScene(string sceneName)
    {
        if (WorldState.Instance.SceneItems.TryGetValue(sceneName, out List<ItemSpawnData> items))
        {
            foreach (var itemData in items)
            {
                SpawnItemObject(itemData.itemID, itemData.position);
            }
        }
    }

    public void SpawnItemObject(string itemID, Vector3 position)
    {
        ItemData itemData = itemDatabaseManager.GetItem(itemID);
        if (itemData != null)
        {
            GameObject itemPrefab = itemData.prefab;
            if (itemPrefab != null)
            {
                Instantiate(itemPrefab, position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning($"Item prefab for item ID '{itemID}' is not assigned.");
            }
        }
        else
        {
            Debug.LogWarning($"Item with ID '{itemID}' not found in the database.");
        }
    }

    public void RemoveItemObject(string itemID)
    {
        Item[] items = FindObjectsByType<Item>();
        foreach (var item in items)
        {
            if (item != null && item.ItemID == itemID)
            {
                Destroy(item.gameObject);
                return;
            }
        }

        Debug.LogWarning($"Item with ID '{itemID}' not found in scene to delete.");
    }
}
