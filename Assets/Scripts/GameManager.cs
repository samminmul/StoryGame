using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int day = 1;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetDay(int newDay)
    {
        day = newDay;
        Debug.Log($"Day set to {day}");
    }

    public void NextDay()
    {
        day++;
        Debug.Log($"Day {day}");
    }
}