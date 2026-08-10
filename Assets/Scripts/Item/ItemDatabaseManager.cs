using UnityEngine;

public class ItemDatabaseManager : MonoBehaviour
{
    public static ItemDatabaseManager Instance;

    [SerializeField]
    private ItemDatabase database;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            database.Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public ItemData GetItem(string itemID)
    {
        return database.GetItem(itemID);
    }
}