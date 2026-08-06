using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private int day = 1;

    private List<Dialogue> dialogues = new List<Dialogue>();

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

    public void NextDay()
    {
        day++;
        Debug.Log($"Day {day}");
    }
}