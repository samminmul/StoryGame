using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class WorldState : MonoBehaviour
{
    public static WorldState Instance;
    ItemSpawner itemSpawner;

    // Dictionary is not supported by Unity serialization analyzers (UAC1009)
    // Mark as non-serialized and expose read-only access if needed.
    [System.NonSerialized]
    private Dictionary<string, List<ItemSpawnData>> sceneItems = new();

    public IReadOnlyDictionary<string, List<ItemSpawnData>> SceneItems => sceneItems;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            itemSpawner = ItemSpawner.instance;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string sceneName, ItemSpawnData itemData)
    {
        if (!sceneItems.ContainsKey(sceneName))
        {
            sceneItems[sceneName] = new List<ItemSpawnData>();
        }
        sceneItems[sceneName].Add(itemData);

        if (sceneName == SceneManager.GetActiveScene().name)
        {
            itemSpawner.SpawnItemObject(itemData.itemID, itemData.position);
        }
    }

    public void RemoveItem(string sceneName, ItemSpawnData itemData)
    {
        if (sceneName == SceneManager.GetActiveScene().name)
        {
            itemSpawner.RemoveItemObject(itemData.itemID);
        }

        if (sceneItems.ContainsKey(sceneName))
        {
            sceneItems[sceneName].Remove(itemData);
        }
    }

    public List<ItemSpawnData> GetItems(string sceneName)
    {
        if (sceneItems.ContainsKey(sceneName))
        {
            return sceneItems[sceneName];
        }
        return new List<ItemSpawnData>();
    }
}