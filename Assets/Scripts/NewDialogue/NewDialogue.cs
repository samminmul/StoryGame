using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class NewDialogue: MonoBehaviour
{
    EventManager eventManager;
    private Dictionary<int, NewDialoguePageData> pages;
    private NewDialoguePageData currentPage;
    private int currentPageNum;
    private int weightSum;

    public NewDialogue()
    {
        pages = new Dictionary<int, NewDialoguePageData>();
        eventManager = EventManager.Instance;
        SetPage(0);
        weightSum = 0;
    }

    private void SetPage(int pageNum)
    {
        currentPageNum = pageNum;
        currentPage = pages[currentPageNum];
    }


    private int FindNextPageNum(int choiceIndex = 0)
    {
        if (currentPage.IsChoicePage())
        {
            return currentPage.GetLines()[choiceIndex].jumpto ?? (currentPageNum + 1); // Return the jumpto of the chosen line, or next page if null
        }
        else
        {
            return currentPage.GetLine().jumpto ?? (currentPageNum + 1); // Return next page for non-choice pages
        }
    }

    private void LineTrigger(NewDialogueLineData line)
    {
        eventManager.TriggerEvent(line.trigger);
        weightSum += line.weight ?? 0;
    }

    private void ExecuteLine(int pageNum, int choiceIndex = 0) // choiceIndex is only used for choice pages, 지정한 line의 각종 부가기능을 실행
    {
        if (!pages.ContainsKey(pageNum))
        {
            Debug.LogError($"ExecuteLine() called with invalid pageNum {pageNum}.");
            return;
        }
        if (currentPage.IsChoicePage())
        {
            var lines = currentPage.GetLines();
            if (choiceIndex < 0 || choiceIndex >= lines.Count)
            {
                Debug.LogError($"ExecuteLine() called with invalid choiceIndex {choiceIndex} for pageNum {pageNum}.");
                return;
            }
            var line = lines[choiceIndex];
            if (!string.IsNullOrEmpty(line.trigger))
            {
                LineTrigger(line);
            }

        }
        else
        {
            var line = currentPage.GetLine();
            if (!string.IsNullOrEmpty(line.trigger))
            {
                LineTrigger(line);
            }
        }
    }

    public void GoToNextPage()
    {
        if (currentPage.IsChoicePage())
        {
            Debug.LogError("GoToNextPage() called on a choice page, use Choose() instead.");
            return;
        }
        ExecuteLine(currentPageNum);
        int nextPageNum = FindNextPageNum();
        SetPage(nextPageNum);
    }

    public void Choose(int choiceIndex)
    {
        if (!currentPage.IsChoicePage())
        {
            Debug.LogError("Choose() called on a non-choice page, use GoToNextPage() instead.");
            return;
        }
        ExecuteLine(currentPageNum, choiceIndex);
        int nextPageNum = FindNextPageNum(choiceIndex);
        SetPage(nextPageNum);
    }

    public bool IsChoicePage()
    {
        return currentPage.IsChoicePage();
    }

    public NewDialogueLineData GetCurrentLine() // For non-choice pages, 데이터 전부 받기
    {
        if (currentPage.IsChoicePage())
        {
            Debug.LogError("GetCurrentLine() called on a choice page, use GetCurrentLines() instead.");
            return null;
        }
        else
        {
            return currentPage.GetLine();
        }
    }

    public List<NewDialogueLineData> GetCurrentLines() // For choice pages, 리스트 형태로 데이터 전부 받기
    {
        if (!currentPage.IsChoicePage())
        {
            Debug.LogError("GetCurrentLines() called on a non-choice page, use GetCurrentLine() instead.");
            return null;
        }
        else
        {
            return currentPage.GetLines();
        }
    }



    public void AddLineToPage(int pageNum, NewDialogueLineData lineData)
    {
        if (!pages.ContainsKey(pageNum))
        {
            pages[pageNum] = new NewDialoguePageData();
        }
        pages[pageNum].lines.Add(lineData);
    }

    public void DataLogForDebug()
    {
        Debug.Log($"NewDialogueData: loaded {pages.Count} pages.");
        foreach (var kv in pages)
        {
            Debug.Log($"Page Number '{kv.Key}' => {kv.Value.lines.Count} line(s)");
            for (int i = 0; i < kv.Value.lines.Count; i++)
            {
                var line = kv.Value.lines[i];
                Debug.Log($"[{kv.Key}][{i}] dialogueCode='{line.dialogueCode}', dialoueNum='{line.dialoueNum}', text='{line.lineKr}', jumpto='{line.jumpto}'");
            }
        }
    }
}
