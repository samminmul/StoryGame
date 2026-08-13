using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Game/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemID;
    public string itemName;
    public GameObject prefab;
}