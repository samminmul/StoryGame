using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class DiaryData : MonoBehaviour
{
    GameManager gm;

    private List<string> diaryText = new List<string>();

    void Start()
    {
        gm = FindAnyObjectByType<GameManager>();
    }

    public void AddDiaryEntry(string entry)
    {
        diaryText.Add(entry);
    }

    public string GetDiaryTextOfDay(int day)
    {
        if (day >= 1 && day <= diaryText.Count)
        {
            return diaryText[day - 1];
        }
        return string.Empty;
    }
}
