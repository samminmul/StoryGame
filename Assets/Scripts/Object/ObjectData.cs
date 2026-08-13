using UnityEngine;

[CreateAssetMenu(fileName = "ObjectData", menuName = "Game/Object Data")]
public class ObjectData : ScriptableObject
{
    public string objectID;
    public string objectName;
    public GameObject prefab;
}