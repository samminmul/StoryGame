using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 대화 진행 로직 스캐폴딩. 엑셀 파싱/이벤트 연결은 팀원이 담당하며,
// 팀원은 파싱된 List<DialogueLineData>를 StartDialogue()에 넘기고
// OnDialogueEnded 이벤트로 최종 결과 코드(AA/AB 등)를 받아 EventManager와 연결하면 된다.
[RequireComponent(typeof(DialogueBoxUI))]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private DialogueBoxUI dialogueBoxUI;

    // TODO: onlywhen(예: happiness>=10) 실제 조건 평가로 교체. 지금은 항상 통과.
    public Func<string, bool> ConditionEvaluator = _ => true;

    public event Action<string> OnDialogueEnded;

    private List<DialogueLineData> currentLines;
    private Dictionary<int, int> lineNumberToIndex;
    private int currentIndex;
    private int weightTotal;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (dialogueBoxUI == null)
        {
            dialogueBoxUI = GetComponent<DialogueBoxUI>();
        }
    }

    public void StartDialogue(List<DialogueLineData> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("DialogueManager: 빈 대화 데이터로 시작할 수 없습니다.");
            return;
        }

        currentLines = lines;
        lineNumberToIndex = new Dictionary<int, int>();
        for (int i = 0; i < lines.Count; i++)
        {
            lineNumberToIndex[lines[i].LineNumber] = i;
        }
        currentIndex = 0;
        weightTotal = 0;
        dialogueBoxUI.Show();
        ShowCurrent();
    }

    private void ShowCurrent()
    {
        if (currentIndex >= currentLines.Count || currentLines[currentIndex].SpeakerCode == "end")
        {
            EndDialogue(ResolveEndCode());
            return;
        }

        List<DialogueLineData> choiceGroup = CollectChoiceGroup(currentIndex);
        if (choiceGroup.Count > 0)
        {
            // TODO: item-code가 있는 선택지는 InventoryManager.HaveItem()으로 필터링 필요.
            dialogueBoxUI.ShowChoices(choiceGroup, OnChoiceSelected);
            return;
        }

        DialogueLineData line = currentLines[currentIndex];
        if (!string.IsNullOrEmpty(line.OnlyWhen) && !ConditionEvaluator(line.OnlyWhen))
        {
            currentIndex++;
            ShowCurrent();
            return;
        }

        dialogueBoxUI.ShowLine(line.SpeakerCode, line.LineKr, OnAdvance);
    }

    // weight가 설정된 연속된 줄들을 하나의 선택지 묶음으로 취급한다.
    private List<DialogueLineData> CollectChoiceGroup(int startIndex)
    {
        var group = new List<DialogueLineData>();
        for (int i = startIndex; i < currentLines.Count && currentLines[i].Weight.HasValue; i++)
        {
            group.Add(currentLines[i]);
        }
        return group;
    }

    private void OnAdvance()
    {
        Advance(currentLines[currentIndex].Jumpto, currentIndex + 1);
    }

    private void OnChoiceSelected(DialogueLineData choice)
    {
        weightTotal += choice.Weight ?? 0;
        Advance(choice.Jumpto, currentIndex + 1);
    }

    private void Advance(string jumpto, int fallbackIndex)
    {
        if (!string.IsNullOrEmpty(jumpto))
        {
            if (int.TryParse(jumpto, out int targetLineNumber) && lineNumberToIndex.TryGetValue(targetLineNumber, out int targetIndex))
            {
                currentIndex = targetIndex;
                ShowCurrent();
                return;
            }

            // 숫자가 아니거나(AA, AB 등) 이 대화 안에 없는 행 번호 = 결과 코드로 바로 종료.
            EndDialogue(jumpto);
            return;
        }

        currentIndex = fallbackIndex;
        ShowCurrent();
    }

    private string ResolveEndCode()
    {
        List<DialogueLineData> endRows = currentLines.Where(l => l.SpeakerCode == "end").ToList();
        if (endRows.Count == 0)
        {
            return null;
        }
        return weightTotal > 0 ? endRows[0].Jumpto : endRows[endRows.Count - 1].Jumpto;
    }

    private void EndDialogue(string resultCode)
    {
        dialogueBoxUI.Hide();
        currentLines = null;
        OnDialogueEnded?.Invoke(resultCode);
    }
}