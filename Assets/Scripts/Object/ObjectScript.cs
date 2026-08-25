using UnityEngine;

public class ObjectScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        // Implement interaction logic here
        Debug.Log($"{gameObject.name} has been interacted with.");
    }

    public void SetLocation()
    {
        // Implement location setting logic here
        Debug.Log($"{gameObject.name} location has been set.");
    }

    public void DestroyObject()
    {
        // Implement object destruction logic here
        Debug.Log($"{gameObject.name} has been destroyed.");
        Destroy(gameObject);
    }
}
