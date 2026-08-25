using UnityEngine;

public class Character : MonoBehaviour
{
    private int happiness;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        happiness = 0; // Initialize happiness to a default value
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeHappiness(int amount)
    {
        happiness += amount;
        Debug.Log($"Character's happiness changed by {amount}. New happiness: {happiness}");
    }
}
