using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class NewDialoguePageData
{
    public List<NewDialogueLineData> lines;

    public NewDialoguePageData()
    {
        lines = new List<NewDialogueLineData>();
    }
    public int? PageNum()
    {
        return lines[0].dialoueNum;
    }

    public int LinesCount()
    {
        return lines.Count;
    }

    public bool IsChoicePage()
    {
        return LinesCount() != 1;
    }

    public bool IsEndPage()
    {
        return lines[0].speakerCode == "end";
    }

    public NewDialogueLineData GetLine()
    {
        if (IsChoicePage())
        {
            Debug.LogError("GetLine() called on a choice page, use GetLines() instead.");
            return null;
        }
        else
        {
            return lines[0];
        }
    }

    public List<NewDialogueLineData> GetLines()
    {
        if (!IsChoicePage())
        {
            Debug.LogError("GetLines() called on a non-choice page, use GetLine() instead.");
            return null;
        }
        else
        {
            return lines;
        }
    }   
}
